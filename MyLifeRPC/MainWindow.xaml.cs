using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RPC;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MyLife
{
    public partial class MainWindow : Window
    {
        #region HotKey
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;

        //Modifiers:
        private const uint MOD_NONE = 0x0000; //(none)
        private const uint MOD_ALT = 0x0001; //ALT
        private const uint MOD_CONTROL = 0x0002; //CTRL
        private const uint MOD_SHIFT = 0x0004; //SHIFT
        private const uint MOD_WIN = 0x0008; //WINDOWS

        private const uint VK_CAPITAL = 0x51;

        private IntPtr _windowHandle;
        private HwndSource _source;
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, VK_CAPITAL);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch(msg)
            {
                case WM_HOTKEY:
                    switch(wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            int vkey = (((int)lParam >> 16) & 0xFFFF);
                            if(vkey == VK_CAPITAL)
                            {
                                OnHotKeyPressed();
                            }
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            base.OnClosed(e);
        }
        #endregion

        private Config config;

        public MainWindow()
        {
            // Don't Open New Windows
            if(Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location)).Count() > 1)
                Process.GetCurrentProcess().Kill();

            Load();
            InitializeComponent();
        }

        private void Load()
        {
            if(File.Exists("../save.json"))
            {
                try
                {
                    var text = File.ReadAllText("../save.json", Encoding.UTF8);
                    config = JsonConvert.DeserializeObject<Config>(text, new IsoDateTimeConverter());
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.Message, "Exception Detected!");
                    config = new Config();
                }
            }
            else
            {
                config = new Config();
            }

            if(config.ClientId == null)
            {
                Hide();
                var win = new FirstTimeWindow(config, this);
            }

            config.Client = new RPC();
            config.Client.Initialize(config.ClientId);
        }

        private void RefreshList()
        {
            templateChooserLb.Items.Clear();

            templateChooserLb.Items.Add(new ListBoxItem()
            {
                Content = "Custom",
                Foreground = Brushes.DarkOrange,
                FontWeight = FontWeights.Bold
            });

            foreach(var temp in config.Templates)
            {
                if(temp.Key == "Custom")
                {
                    continue;
                }
                templateChooserLb.Items.Add(new ListBoxItem()
                {
                    Content = temp.Key
                });
            }
        }

        private void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(config, Formatting.Indented, new IsoDateTimeConverter());
                File.WriteAllText("../save.json", json, Encoding.UTF8);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "Exception Detected!");
            }
        }

        private void OnHotKeyPressed()
        {
            Visibility ^= Visibility.Hidden; // Show/Hide
        }

        private void isOnBtn_Click(object sender, RoutedEventArgs e)
        {
            config.IsOn ^= true;
            if(config.IsOn)
            {
                isOnBtn.Content = "On";
                isOnBtn.Foreground = Brushes.DarkGreen;
                isOnBtn.Background = Brushes.LightGreen;
                if(config.Client.client == null || config.Client.client.Disposed)
                {
                    config.Client.Initialize(config.ClientId);
                }
                else if(!config.Client.client.IsInitialized)
                {
                    config.Client.client.Initialize();
                }
            }
            else
            {
                isOnBtn.Content = "Off";
                isOnBtn.Foreground = Brushes.DarkRed;
                isOnBtn.Background = Brushes.IndianRed;

                if(config.Client.client.IsInitialized)
                {
                    config.Client.client.Dispose();
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Save();
        }

        private void addTemplateTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(addTemplateTxt.Text.Length > 0 && !config.Templates.ContainsKey(addTemplateTxt.Text) && addTemplateTxt.Text.ToLower() != "config")
            {
                addTemplateBtn.IsEnabled = true;
            }
            else
            {
                addTemplateBtn.IsEnabled = false;
            }
        }

        private void addTemplateBtn_Click(object sender, RoutedEventArgs e)
        {
            config.Templates.Add(addTemplateTxt.Text, new RichPresence()
            {
                Details = addTemplateTxt.Text,
                Timestamps = new Timestamps()
                {
                    Start = DateTime.UtcNow
                },
                Assets = new Assets(),
                Party = new Party()
            });
            var itm = new ListBoxItem()
            {
                Content = addTemplateTxt.Text
            };
            templateChooserLb.Items.Add(itm);
            addTemplateTxt.Text = string.Empty;
        }

        private void addTemplateTxt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter && addTemplateTxt.Text.Length > 0 && !config.Templates.ContainsKey(addTemplateTxt.Text) && addTemplateTxt.Text.ToLower() != "config")
            {
                addTemplateBtn_Click(null, null);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void templateChooserLb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(templateChooserLb.SelectedItems.Count < 1)
            {
                removeTemplateBtn.IsEnabled = false;
                return;
            }

            var sel = (ListBoxItem)templateChooserLb.SelectedItem;
            var key = (string)sel.Content;

            var temp = config.Templates[key];
            config.Client.SetPresence(temp);
            removeTemplateBtn.IsEnabled = key != "Custom";

            titleTxt.Text = temp.Details;
            descriptionTxt.Text = temp.State;

            largeImageKeyTxt.Text = temp.Assets?.LargeImageKey;
            largeImageTextTxt.Text = temp.Assets?.LargeImageText;
            smallImageKeyTxt.Text = temp.Assets?.SmallImageKey;
            smallImageTextTxt.Text = temp.Assets?.SmallImageText;


            if(temp.Timestamps.Start != null)
            {
                startTime.Text = (temp.Timestamps.Start ?? DateTime.UtcNow).ToLocalTime().ToString("dd/MM/yy HH:mm:ss");
            }
            else
            {
                endTime.Text = "";
            }

            if(temp.Timestamps.End.HasValue)
            {
                endTime.Text = (temp.Timestamps.Start ?? DateTime.UtcNow).ToLocalTime().ToString("dd/MM/yy HH:mm:ss");
            }
            else
            {
                endTime.Text = "";
            }

            partySizeNum.Text = temp.Party is null ? "" : temp.Party.Size.ToString();
            partyMaxNum.Text = temp.Party is null ? "" : temp.Party.Max.ToString();


        }

        private void removeTemplateBtn_Click(object sender, RoutedEventArgs e)
        {
            var sel = (ListBoxItem)templateChooserLb.SelectedItem;
            templateChooserLb.SelectedIndex = templateChooserLb.SelectedIndex - 1;
            templateChooserLb.Items.Remove(sel);
            config.Templates.Remove((string)sel.Content);
        }

        private void startTimeNowBtn_Click(object sender, RoutedEventArgs e)
        {
            startTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }

        private void startTimeClearBtn_Click(object sender, RoutedEventArgs e)
        {
            startTime.Text = "";
        }

        private void endTimeNowBtn_Click(object sender, RoutedEventArgs e)
        {
            endTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }

        private void endTimeClearBtn_Click(object sender, RoutedEventArgs e)
        {
            endTime.Text = "";
        }

        private void partyClearBtn_Click(object sender, RoutedEventArgs e)
        {
            partySizeNum.Text = "";
            partyMaxNum.Text = "";
        }

        private void titleTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(templateChooserLb.SelectedItems.Count > 0)
            {
                var tem = (ListBoxItem)templateChooserLb.SelectedItem;
                var key = (string)tem.Content;
                config.Templates[key].Details = titleTxt.Text;
                config.Client.SetPresence(config.Templates[key]);
            }
        }

        private void descriptionTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(templateChooserLb.SelectedItems.Count > 0)
            {
                var tem = (ListBoxItem)templateChooserLb.SelectedItem;
                var key = (string)tem.Content;
                config.Templates[key].State = descriptionTxt.Text;
                config.Client.SetPresence(config.Templates[key]);
            }
        }

        private void largeImageKeyTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(templateChooserLb.SelectedItems.Count > 0)
            {
                var tem = (ListBoxItem)templateChooserLb.SelectedItem;
                var key = (string)tem.Content;
                if(config.Templates[key].Assets == null)
                {
                    config.Templates[key].Assets = new Assets();
                }
                config.Templates[key].Assets.LargeImageKey = largeImageKeyTxt.Text;
                config.Client.SetPresence(config.Templates[key]);
            }
        }

        private void largeImageTextTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(templateChooserLb.SelectedItems.Count > 0)
            {
                var tem = (ListBoxItem)templateChooserLb.SelectedItem;
                var key = (string)tem.Content;
                if(config.Templates[key].Assets == null)
                {
                    config.Templates[key].Assets = new Assets();
                }
                config.Templates[key].Assets.LargeImageText = largeImageTextTxt.Text;
                config.Client.SetPresence(config.Templates[key]);
            }
        }

        private void smallImageKeyTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(templateChooserLb.SelectedItems.Count > 0)
            {
                var tem = (ListBoxItem)templateChooserLb.SelectedItem;
                var key = (string)tem.Content;
                if(config.Templates[key].Assets == null)
                {
                    config.Templates[key].Assets = new Assets();
                }
                config.Templates[key].Assets.SmallImageKey = smallImageKeyTxt.Text;
                config.Client.SetPresence(config.Templates[key]);
            }
        }

        private void smallImageTextTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(templateChooserLb.SelectedItems.Count > 0)
            {
                var tem = (ListBoxItem)templateChooserLb.SelectedItem;
                var key = (string)tem.Content;
                if(config.Templates[key].Assets == null)
                {
                    config.Templates[key].Assets = new Assets();
                }
                config.Templates[key].Assets.SmallImageText = smallImageTextTxt.Text;
                config.Client.SetPresence(config.Templates[key]);
            }
        }

        private void startTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(templateChooserLb.SelectedItems.Count > 0)
            {
                var tem = (ListBoxItem)templateChooserLb.SelectedItem;
                var key = (string)tem.Content;
                if(config.Templates[key].Timestamps == null)
                {
                    config.Templates[key].Timestamps = new Timestamps();
                }
                if(startTime.Value is null)
                {
                    config.Templates[key].Timestamps.Start = null;
                }
                else
                {
                    config.Templates[key].Timestamps.Start = ((DateTime)startTime.Value).ToUniversalTime();
                }
                config.Client.SetPresence(config.Templates[key]);
            }
        }

        private void endTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(templateChooserLb.SelectedItems.Count > 0)
            {
                var tem = (ListBoxItem)templateChooserLb.SelectedItem;
                var key = (string)tem.Content;
                if(config.Templates[key].Timestamps == null)
                {
                    config.Templates[key].Timestamps = new Timestamps();
                }
                if(endTime.Value is null)
                {
                    config.Templates[key].Timestamps.End = null;
                }
                else
                {
                    config.Templates[key].Timestamps.End = ((DateTime)endTime.Value).ToUniversalTime();
                }
                config.Client.SetPresence(config.Templates[key]);
            }
        }

        private void partySizeNum_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(templateChooserLb.SelectedItems.Count > 0)
            {
                var tem = (ListBoxItem)templateChooserLb.SelectedItem;
                var key = (string)tem.Content;
                if(config.Templates[key].Party == null)
                {
                    config.Templates[key].Party = new Party();
                }
                config.Templates[key].Party.Size = Convert.ToInt32(partySizeNum.Value);
                config.Client.SetPresence(config.Templates[key]);
            }
        }

        private void partyMaxNum_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(templateChooserLb.SelectedItems.Count > 0)
            {
                var tem = (ListBoxItem)templateChooserLb.SelectedItem;
                var key = (string)tem.Content;
                if(config.Templates[key].Party == null)
                {
                    config.Templates[key].Party = new Party();
                }
                config.Templates[key].Party.Max = Convert.ToInt32(partyMaxNum.Value);
                config.Client.SetPresence(config.Templates[key]);
            }
        }
    }
}
