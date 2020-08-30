using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft;

namespace PlotWizard.Ribbon
{
    [Serializable]
    public class Settings
    {
        public string PlotterType { get; set; } = "DWG To PDF.pc3";
        // key = readable name, value = canonical name
        public KeyValuePair<string, string> PageSize { get; set; } = new KeyValuePair<string, string>("ISO без полей A4(297.00 x 210.00 мм)", "ISO_full_bleed_A4_(210.00_x_297.00_MM)");
        public double ContentScaling { get; set; } = 1.003;
        public double ViewportScaling { get; set; } = 1;
        public bool AutoOpenFile { get; set; } = true;
    }

    internal static class LayoutSettings
    {
        public const string configFileName = "plotwizardconfig.json";
        public static Settings Default { get; private set; } = new Settings();
        public static Settings Current { get; private set; } = new Settings();
        private static Settings Loading = new Settings();
        public static void SetDefaults()
        {
            
            if (LoadConfig(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\" + configFileName))
            {
                Default = Loading;
            }
            Current = Default;
        }
        public static void SaveConfig(string outputFileName)
        {
            /*
            if (String.IsNullOrEmpty(outputFileName) || String.IsNullOrWhiteSpace(outputFileName))
            {
                System.Windows.MessageBox.Show("Не удалось сохранить файл конфигурации. Неправильно задан путь к файлу.");
                return;
            }

            if (new FileInfo(outputFileName).Exists)
            {

                string str = JsonSerializer.Serialize(Default, new JsonSerializerOptions { WriteIndented = true });
                /*
                var writer = new JsonObject
                    {
                        { nameof(Defaults.PlotterType), Current.PlotterType },
                        { nameof(Defaults.PageSize), Current.PageSize.Key },
                        { nameof(Defaults.ContentScaling), Current.ContentScaling.ToString() },
                        { nameof(Defaults.ViewportScaling), Current.ViewportScaling.ToString() },
                        { nameof(Defaults.AutoOpenFile), Current.AutoOpenFile.ToString() }
                    };

                string str = writer.ToString();
                */
                /*
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
             */
        }

        public static bool LoadConfig(string inputFileName)
        {
            return false;
            /*
            if (String.IsNullOrEmpty(inputFileName) || String.IsNullOrWhiteSpace(inputFileName) || !(new FileInfo(inputFileName).Exists))
            {
                System.Windows.MessageBox.Show("Не удалось загрузить файл конфигурации. Неправильно задан путь к файлу.");
                return false;
            }
            JsonValue.Parse("");
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

            if (!String.IsNullOrEmpty(str))
            {
                JsonArray values = new JsonArray()
                try
                {
                    values = JsonArray.Parse(str);
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(e.ToString());
                    return false;
                }

                bool err = false;
                string readablePageSize = null;

                System.Windows.MessageBox.Show(values.ToString());

                foreach (KeyValuePair<string, JsonObject> obj in values)
                {
                    switch (obj.Key)
                    {
                        case nameof(Defaults.AutoOpenFile):
                            if (bool.TryParse(obj.Value.ToString(), out bool autoopen))
                                Loading.AutoOpenFile = autoopen;
                            else
                                err = true;
                            break;
                        case nameof(Defaults.ContentScaling):
                            if (double.TryParse(obj.Value.ToString(), out double cscale))
                                Loading.ContentScaling = Extensions.Clamp(cscale, 0, 999);
                            else
                                err = true;
                            break;
                        case nameof(Defaults.ViewportScaling):
                            if (double.TryParse(obj.Value.ToString(), out double vscale))
                                Loading.ViewportScaling = Extensions.Clamp(vscale, 0, 999);
                            else
                                err = true;
                            break;
                        case nameof(Defaults.PageSize):
                            string psize = obj.Value.ToString();
                            if (!string.IsNullOrEmpty(psize) && !string.IsNullOrEmpty(psize))
                                readablePageSize = psize;
                            else
                                err = true;
                            break;
                        case nameof(Defaults.PlotterType):
                            string pltype = obj.Value.ToString();
                            if (!string.IsNullOrEmpty(pltype) && !string.IsNullOrEmpty(pltype))
                                Loading.PlotterType = pltype;
                            else
                                err = true;
                            break;
                        default:
                            err = true;
                            break;
                    }

                    if (err)
                    {
                        System.Windows.MessageBox.Show("Не удалось загрузить файл конфигурации. Ошибка чтения параметров.");
                        return false;
                    }

                }

                if (!string.IsNullOrEmpty(readablePageSize) && !string.IsNullOrWhiteSpace(readablePageSize))
                {
                    string canonicalPageSize;
                    try
                    {
                        canonicalPageSize = Extensions.GetMediaNameList(Defaults.PlotterType)[readablePageSize];
                    }
                    catch (Exception)
                    {
                        System.Windows.MessageBox.Show("Не удалось загрузить файл конфигурации. Ошибка чтения параметров.");
                        return false;
                    }
                    Loading.PageSize = new KeyValuePair<string, string>(readablePageSize, canonicalPageSize);
                    return true;
                }
                else
                {
                    System.Windows.MessageBox.Show("Не удалось загрузить файл конфигурации. Ошибка чтения параметров.");
                    return false;
                }
                
            }
            else
                return false;
                        */
        }
    }
}
