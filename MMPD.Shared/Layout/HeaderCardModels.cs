namespace MMPD.Shared.Layout
{
    public class HeaderCardModels
    {
        public class HeadInfoCardModel
        {
            public string Title { get; set; } = "";
            public string Subtitle1 { get; set; } = "";
            public string Subtitle2 { get; set; } = "";
            public string Subtitle3 { get; set; } = "";
            public string Subtitle4 { get; set; } = "";
            public string Subtitle5 { get; set; } = "";
            public string Subtitle6 { get; set; } = "";
            public string Subtitle7 { get; set; } = "";
            public string Subtitle8 { get; set; } = "";
            public string Phone { get; set; } = "";
            public string Phone2 { get; set; } = "";
            public string Phone3 { get; set; } = "";
            public string Email { get; set; } = "";
            public string Address { get; set; } = "";
            public string City { get; set; } = "";
            public string State { get; set; } = "";
            public string Zip { get; set; } = "";
            public string StoreManager { get; set; } = "";
            public string AreaManager { get; set; } = "";
            public string TManager1 { get; set; } = "";
            public string TManager2 { get; set; } = "";
            public string TManager3 { get; set; } = "";
            public string TManager4 { get; set; } = "";
            public string TManager5 { get; set; } = "";
            public string TManager6 { get; set; } = "";
            public string TManager7 { get; set; } = "";
            public string TManager8 { get; set; } = "";
            public string ShowBackButton { get; set; } = "true";

        }

        public static HeadInfoCardModel Corporate()
        {
            return new HeadInfoCardModel
            {
                Title = "Employee Directory",
                Address = "1500 Hamilton Rd",
                City = "Bossier City",
                State = "LA",
                Zip = "71111",
                Subtitle1 = "Corp. Phone",
                Phone = "(888) 245-3696",
                Subtitle2 = "Corp. Email",
                Email = "info@mcelroymetal.com",
                Subtitle3 = "IT Help Desk",
                Phone2 = "(866) 634-5111",

            };
        }

        public static HeadInfoCardModel ForDepartment(string title, string? phone, string? email, string? manager)
        {
            return new HeadInfoCardModel
            {
                Title = title,
                Subtitle1 = "Dept. Phone",
                Phone = phone ?? "",
                Subtitle2 = "Dept. Email",
                Email = email ?? "",
                Subtitle3 = "Dept. Manager",
                Phone2 = manager ?? ""
            };
        }

        public static HeadInfoCardModel Plant(string title, string? subtitle1, string? tmanager1, string? subtitle2, string? tmanager2, string? subtitle3, string? tmanager3, string? subtitle4, string? tmanager4, string? subtitle5, string? tmanager5, string? subtitle6, string? tmanager6)
        {
            return new HeadInfoCardModel
            {
                Title = title,
                Subtitle1 = subtitle1 ?? "",
                TManager1 = tmanager1 ?? "",
                Subtitle2 = subtitle2 ?? "",
                TManager2 = tmanager2 ?? "",
                Subtitle3 = subtitle3 ?? "",
                TManager3 = tmanager3 ?? "",
                Subtitle4 = subtitle4 ?? "",
                TManager4 = tmanager4 ?? "",
                Subtitle5 = subtitle5 ?? "",
                TManager5 = tmanager5 ?? "",
                Subtitle6 = subtitle6 ?? "",
                TManager6 = tmanager6 ?? "",
            };
        }

        public static HeadInfoCardModel Employee(string title)
        {
            return new HeadInfoCardModel
            {
                Title = title
            };
        }

        public static HeadInfoCardModel Store(string title, string? subtitle1, string? tmanager1, string? subtitle2, string? tmanager2, string? subtitle3, string? tmanager3, string? subtitle4, string? tmanager4, string? subtitle5, string? tmanager5, string? subtitle6, string? tmanager6)
        {
            return new HeadInfoCardModel
            {
                Title = title,
                Subtitle1 = subtitle1 ?? "",
                TManager1 = tmanager1 ?? "",
                Subtitle2 = subtitle2 ?? "",
                TManager2 = tmanager2 ?? "",
                Subtitle3 = subtitle3 ?? "",
                TManager3 = tmanager3 ?? "",
                Subtitle4 = subtitle4 ?? "",
                TManager4 = tmanager4 ?? "",
                Subtitle5 = subtitle5 ?? "",
                TManager5 = tmanager5 ?? "",
                Subtitle6 = subtitle6 ?? "",
                TManager6 = tmanager6 ?? "",
            };
        }
    }
}
