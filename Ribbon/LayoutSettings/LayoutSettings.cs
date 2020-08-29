using System;
using System.Collections.Generic;
using System.IO;
using System.Json;

namespace PlotWizard.Ribbon
{
    internal static class LayoutSettings
    {
        public const string configFileName = "plotwizardconfig.json";
        public static string defaultPlotterType { get; private set; } = "DWG To PDF.pc3";
        // key = readable name, value = canonical name
        public static KeyValuePair<string, string> defaultPageSize { get; private set; } = new KeyValuePair<string, string>("ISO без полей A4(297.00 x 210.00 мм)", "ISO_full_bleed_A4_(210.00_x_297.00_MM)");
        public static double defaultContentScaling { get; private set; } = 1.003;
        public static double defaultViewportScaling { get; private set; } = 1;
        public static bool defaultAutoOpenFile { get; private set; } = true;
        public static string PlotterType { get; set; } = null;
        // key = readable name, value = canonical name
        public static KeyValuePair<string, string> PageSize { get; set; } = new KeyValuePair <string, string> (null, null);
        public static double ContentScaling { get; set; } = -1;
        public static double ViewportScaling { get; set; } = -1;
        public static bool? AutoOpenFile { get; set; } = null;
        public static void SetDefaults()
        {
            if (LoadConfig(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\" + configFileName))
            {

            }

            PlotterType = defaultPlotterType;
            PageSize = defaultPageSize;
            ContentScaling = defaultContentScaling;
            ViewportScaling = defaultViewportScaling;
            AutoOpenFile = defaultAutoOpenFile;
        }
        public static void SaveConfig(string outputFileName)
        {
            if (String.IsNullOrEmpty(outputFileName) || String.IsNullOrWhiteSpace(outputFileName))
            {
                System.Windows.MessageBox.Show("Не удалось сохранить файл конфигурации. Неправильно задан путь к файлу.");
                return;
            }

            if (new FileInfo(outputFileName).Exists)
            {
                var writer = new JsonObject
                    {
                        { nameof(defaultPlotterType), PlotterType },
                        { nameof(defaultPageSize), PageSize.Key },
                        { nameof(defaultContentScaling), ContentScaling.ToString() },
                        { nameof(defaultViewportScaling), ViewportScaling.ToString() },
                        { nameof(defaultAutoOpenFile), AutoOpenFile.ToString() }
                    };
                string str = writer.ToString();

                if (String.IsNullOrEmpty(str))
                {
                    System.Windows.MessageBox.Show("Не удалось сохранить файл конфигурации. Конфигурация пуста.");
                    return;
                }

                try
                {
                    File.WriteAllText(outputFileName, str);
                }
                catch (IOException)
                {
                    System.Windows.MessageBox.Show("Не удалось сохранить файл конфигурации. Ошибка сохранения файла по заданному пути.");
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Не удалось сохранить файл конфигурации. Ошибка открытия файла по заданному пути.");
            }
        }
    
    public static bool LoadConfig(string inputFileName)
        {
            if (String.IsNullOrEmpty(inputFileName) || String.IsNullOrWhiteSpace(inputFileName) || !(new FileInfo(inputFileName).Exists))
            {
                System.Windows.MessageBox.Show("Не удалось загрузить файл конфигурации. Неправильно задан путь к файлу.");
                return false;
            }

            string str;

            try
            {
                str = File.ReadAllText(inputFileName);
            }
            catch (IOException)
            {
                System.Windows.MessageBox.Show("Не удалось загрузить файл конфигурации. Ошибка чтения файла.");
                return false;
            }

            try
            {
                if (!String.IsNullOrEmpty(str))
                {
                    var values = JsonArray.Parse(str);
                    foreach (KeyValuePair<string, JsonObject> obj in values)
                    {
                        System.Windows.MessageBox.Show(obj.Key + " " + obj.Value.ToString());
                    }
                }
            }
            catch (Exception e )
            {
                System.Windows.MessageBox.Show(e.ToString());
            }
            
            return false;
        }
    }
}
