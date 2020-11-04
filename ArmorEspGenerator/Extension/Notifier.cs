using System;
using System.Drawing;

public static class Notifier
{
    public static Pen arcOutLinePen = new Pen(Color.SteelBlue, 4);
    public static Pen arcPen = new Pen(Color.Lime, 2);
    public static Brush fontColor = new SolidBrush(Color.Black);
    public static Rectangle ArcRect = new Rectangle(1, 1, 13, 13);
    public static Font espFont = new Font("Microsoft Sans Serif", 10, FontStyle.Italic, GraphicsUnit.Pixel);
    public static StringFormat stringFormat = new StringFormat()
    {
        LineAlignment = StringAlignment.Center,
        Alignment = StringAlignment.Center,
        Trimming = StringTrimming.None,
        FormatFlags = StringFormatFlags.NoWrap
    };
    public static void SetProcessIcon(this System.Windows.Forms.NotifyIcon notifyIcon, double normalize)
    {
        var r = new Rectangle(0, 0, 16, 16);
        Bitmap bitmapText = new Bitmap(r.Width, r.Height);
        Graphics g = Graphics.FromImage(bitmapText);
        IntPtr hIcon;

        //g.Clear(Color.Transparent);
        int angle = (int)(normalize * 360);
        g.DrawArc(arcOutLinePen, ArcRect, 90, angle + 15);
        g.DrawArc(arcPen, ArcRect, 90, angle);

        //g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
        //var txt = normalize >=1 ? "OK" : ((int)(normalize * 100)).ToString();        
        //g.DrawString(txt, fontToUse, new SolidBrush(Color.Black), new Rectangle(0,0,15,15), sf);
        g.DrawString("Esp", espFont, fontColor, r, stringFormat);


        hIcon = (bitmapText.GetHicon());
        if (notifyIcon.Icon != null) notifyIcon.Icon.Dispose();
        notifyIcon.Icon = Icon.FromHandle(hIcon);
        //DestroyIcon(hIcon.ToInt32);
        
    }
}
