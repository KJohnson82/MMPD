using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MMPD.Shared.Components;
using MMPD.Shared.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMPD.Shared.Components.FooterNav;
using static MMPD.Shared.Components.HeaderCard;

namespace MMPD.Shared.Services
{
    //public class LayoutState
    //{
    //    public HeaderCard.HeadInfoCardModel? HeaderInfo { get; set; }
    //    public List<FooterNav.FooterItem> FooterItems { get; set; } = new();
    //    public event Action? OnChange;

    //    public void SetHeader(HeaderCard.HeadInfoCardModel info)
    //    {
    //        HeaderInfo = info;
    //        OnChange?.Invoke();
    //    }

    //    public void SetFooter(List<FooterNav.FooterItem> items)
    //    {
    //        FooterItems = items;
    //        OnChange?.Invoke();
    //    }
    //}

    public class LayoutState
    {
        public HeadInfoCardModel? HeaderInfo { get; private set; }
        public List<FooterItem> FooterItems { get; private set; } = new();

        public event Action? OnChange;

        public void SetHeader(HeadInfoCardModel? info)
        {
            // Only notify listeners if the data has actually changed.
            if (HeaderInfo != info)
            {
                HeaderInfo = info;
                NotifyStateChanged();
            }
        }

        public void SetFooter(List<FooterItem> items)
        {
            FooterItems = items;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
}

}
