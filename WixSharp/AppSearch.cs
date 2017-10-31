using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using IO = System.IO;

namespace WixSharp.CommonTasks
{
    /// <summary>
    /// The utility class implementing the common 'MSI AppSearch' tasks (Directory, File, Registry and Product searches).
    /// </summary>
    public static class AppSearch
    {
        [DllImport("msi", CharSet = CharSet.Unicode)]
        static extern Int32 MsiGetProductInfo(string product, string property, [Out] StringBuilder valueBuf, ref Int32 len);

        [DllImport("msi")]
        static extern int MsiEnumProducts(int iProductIndex, StringBuilder lpProductBuf);

        /// <summary>
        /// Gets the 'product code' of the installed product.
        /// </summary>
        /// <param name="name">The product name.</param>
        /// <returns></returns>
        static public string[] GetProductCode(string name)
        {
            var result = new List<string>();

            var productCode = new StringBuilder();

            int i = 0;
            while (0 == MsiEnumProducts(i++, productCode))
            {
                var productNameLen = 512;
                var productName = new StringBuilder(productNameLen);

                MsiGetProductInfo(productCode.ToString(), "ProductName", productName, ref productNameLen);

                if (productName.ToString() == name)
                    result.Add(productCode.ToString());
            }

            return result.ToArray();
        }

        /// <summary>
        /// Gets array of 'product codes' (GUIDs) of all installed products.
        /// </summary>
        static public string[] GetProducts()
        {
            var result = new List<string>();

            var productCode = new StringBuilder();

            int i = 0;
            while (0 == MsiEnumProducts(i++, productCode))
            {
                var productNameLen = 512;
                var productName = new StringBuilder(productNameLen);
                result.Add(productCode.ToString());
            }

            return result.ToArray();
        }

        /// <summary>
        /// Gets the 'product name' of the installed product.
        /// </summary>
        /// <param name="productCode">The product code.</param>
        /// <returns></returns>
        static public string GetProductName(string productCode)
        {
            var productNameLen = 512;
            var productName = new StringBuilder(productNameLen);
            if (0 == MsiGetProductInfo(productCode, "ProductName", productName, ref productNameLen))
                return productName.ToString();
            else
                return null;
        }

        /// <summary>
        /// Determines whether the product is installed.
        /// </summary>
        /// <param name="productCode">The product code.</param>
        /// <returns></returns>
        static public bool IsProductInstalled(string productCode)
        {
            var productNameLen = 512;
            var productName = new StringBuilder(productNameLen);
            if (0 == MsiGetProductInfo(productCode, "ProductName", productName, ref productNameLen))
                return !string.IsNullOrEmpty(productName.ToString());
            else
                return false;
        }

        /// <summary>
        /// Determines whether the file exists.
        /// </summary>
        /// <param name="file">The file path.</param>
        /// <returns></returns>
        static public bool FileExists(string file)
        {
            try
            {
                return IO.File.Exists(file);
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Determines whether the dir exists.
        /// </summary>
        /// <param name="dir">The directory path.</param>
        /// <returns></returns>
        static public bool DirExists(string dir)
        {
            try
            {
                return IO.Directory.Exists(dir);
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Converts INI file content into dictionary.
        /// </summary>
        /// <param name="iniFile">The INI file.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        static public Dictionary<string, Dictionary<string, string>> IniFileToDictionary(string iniFile, Encoding encoding = null)
        {
            return IniToDictionary(IO.File.ReadAllLines(iniFile, encoding ?? Encoding.Default));
        }

        static internal Dictionary<string, Dictionary<string, string>> IniToDictionary(string[] iniFileContent)
        {
            var result = new Dictionary<string, Dictionary<string, string>>();

            var section = "default";
            var entries = iniFileContent.Select(l => l.Trim())
                                        .Where(l => !l.StartsWith(";") && l.IsNotEmpty());

            foreach (var line in entries)
            {
                if (line.StartsWith("["))
                {
                    section = line.Trim('[', ']');
                    continue;
                }

                if (!result.ContainsKey(section))
                    result[section] = new Dictionary<string, string>();

                var pair = line.Split('=').Select(x => x.Trim());
                if (pair.Count() < 2)
                    result[section][pair.First()] = null;
                else
                    result[section][pair.First()] = pair.Last();
            }

            return result;
        }

        /// <summary>
        /// Returns INI file the field value.
        /// <para>It returns null if file or the field not found.</para>
        /// </summary>
        /// <param name="file">The INI file path.</param>
        /// <param name="section">The section.</param>
        /// <param name="field">The field.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        static public string IniFileValue(string file, string section, string field, Encoding encoding = null)
        {
            try
            {
                return IniFileToDictionary(file)[section][field];
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Determines whether the registry key exists.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="keyPath">The key path.</param>
        /// <returns></returns>
        static public bool RegKeyExists(RegistryKey root, string keyPath)
        {
            using (RegistryKey key = root.OpenSubKey(keyPath))
                return (key != null);
        }

        /// <summary>
        /// Gets the registry value.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="keyPath">The key path.</param>
        /// <param name="valueName">Name of the value.</param>
        /// <returns></returns>
        static public object GetRegValue(RegistryKey root, string keyPath, string valueName)
        {
            using (RegistryKey key = root.OpenSubKey(keyPath))
                if (key != null)
                    return key.GetValue(valueName);
            return null;
        }
    }
}