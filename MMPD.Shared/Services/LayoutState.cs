using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMPD.Shared.Components;
using MMPD.Shared.Layout;

namespace MMPD.Shared.Services
{
    public class LayoutState
    {
        public HeaderCard.HeadInfoCardModel? HeaderInfo { get; set; }
        public List<FooterNav.FooterItem> FooterItems { get; set; } = new();
        public event Action? OnChange;

        public void SetHeader(HeaderCard.HeadInfoCardModel info)
        {
            HeaderInfo = info;
            OnChange?.Invoke();
        }

        public void SetFooter(List<FooterNav.FooterItem> items)
        {
            FooterItems = items;
            OnChange?.Invoke();
        }
    }
}
