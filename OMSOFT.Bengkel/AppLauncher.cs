// AppLauncher.cs: Entry point aplikasi. Jangan menggunakan kelas "Program", tapi "AppLauncher", supaya
//                 intellisense VS ketika mencari App* dapat merujuk langsung ke kelas-kelas utama aplikasi,
//                 semisal AppBrowser, AppLauncher, dll.

namespace OMSOFT.Bengkel
{
    using System;
    using System.Windows.Forms;

    public static partial class AppLauncher
    {
        private static Boolean m_IsRunning;

        public static Boolean IsRunning => m_IsRunning;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            DevExpress.XtraEditors.WindowsFormsSettings.FocusRectStyle = DevExpress.Utils.Paint.DXDashStyle.None;
            m_IsRunning = true;
            Application.Run(new AppTestForm());
        }
    }}
