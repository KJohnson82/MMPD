namespace MMPD
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            const int appHeight = 850;
            const int appWidth = 450;

            var appWindow = new Window(new MainPage())
            {
                Title = "MMPD",
                Height = appHeight,
                Width = appWidth
                
            };

            return appWindow;

            //return new Window(new MainPage()) { Title = "MMPD" };
        }
    }
}
