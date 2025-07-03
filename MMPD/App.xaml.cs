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
            const int appHeight = 900;
            const int appWidth = 450;

            var appWindow = new Window(new MainPage())
            {
                Title = "McElroy Directory",
                Height = appHeight,
                Width = appWidth,
                FlowDirection = FlowDirection.MatchParent,
                TitleBar = new TitleBar
                {
                    Title = "McElroy Directory",
                    Background = Colors.Transparent,
                    ForegroundColor = Colors.Coral

                }
                
                
            };

            return appWindow;

            //return new Window(new MainPage()) { Title = "MMPD" };
        }
    }
}
