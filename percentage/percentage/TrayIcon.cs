using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace percentage
{
    class TrayIcon
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool DestroyIcon(IntPtr handle);

        private const int fontSize = 18;
        private const string font = "Segoe UI";

        private NotifyIcon notifyIcon;

        public TrayIcon()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem menuItem = new ToolStripMenuItem();

            notifyIcon = new NotifyIcon();

            contextMenu.Items.AddRange(new ToolStripMenuItem[] { menuItem });

            menuItem.Click += new System.EventHandler(MenuItemClick);
            menuItem.MergeIndex = 0;
            menuItem.Text = "E&xit";

            notifyIcon.ContextMenuStrip = contextMenu;
            notifyIcon.Visible = true;

            Timer timer = new Timer();
            timer.Interval = /*60**/5000; // Uncomment the 60* to decrease ticks to once per 5 minutes.
            timer.Tick += new EventHandler(TimerTick);
            timer.Start();
        }

        private Bitmap GetTextBitmap(String text, Font font, Color fontColor)
        {
            SizeF imageSize = GetStringImageSize(text, font);
            Bitmap bitmap = new Bitmap((int)imageSize.Width, (int)imageSize.Height);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.FromArgb(0, 0, 0, 0));
                using (Brush brush = new SolidBrush(fontColor))
                {
                    graphics.DrawString(text, font, brush, 0, 0);
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    graphics.Save();
                }
            }
            return bitmap;
        }

        private static SizeF GetStringImageSize(string text, Font font)
        {
            using (Image image = new Bitmap(1, 1))
            using (Graphics graphics = Graphics.FromImage(image))
                return graphics.MeasureString(text, font);
        }

        private void MenuItemClick(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            PowerStatus powerStatus = SystemInformation.PowerStatus;
            String percentage = (powerStatus.BatteryLifePercent * 100).ToString();
            bool isCharging = SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online;
            String bitmapText = percentage;

            Color whiteColor = Color.White;
            Color greenColor = System.Drawing.ColorTranslator.FromHtml("#7CFC00");
            Color redColor = Color.Red;
            Color color;

            if (isCharging)
            {
                color = greenColor;
            }
            else
            {
                if (Int32.Parse(percentage) <= 30)
                {
                    color = redColor;
                }
                else
                {
                    color = whiteColor;
                }
            }

            using (Bitmap bitmap = new Bitmap(GetTextBitmap(bitmapText, new Font(font, fontSize), color)))
            {
                System.IntPtr intPtr = bitmap.GetHicon();
                try
                {
                    using (Icon icon = Icon.FromHandle(intPtr))
                    {
                        notifyIcon.Icon = icon;
                        String toolTipText = percentage + "%" + (isCharging ? " Charging" : "");
                        notifyIcon.Text = toolTipText;
                    }
                }
                finally
                {
                    DestroyIcon(intPtr);
                }
            }
        }
    }
}
