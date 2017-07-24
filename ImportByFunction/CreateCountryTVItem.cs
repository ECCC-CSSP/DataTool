using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSSPEnumsDLL.Enums;
using CSSPModelsDLL.Models;
using CSSPWebToolsDBDLL.Services;

namespace ImportByFunction
{
    public partial class ImportByFunction
    {
        private bool CreateCountryTVItem()
        {
            if (Cancel) return false;

            lblStatus.Text = "CreateCountryTVItem ... ";
            lblStatus.Refresh();
            Application.DoEvents();

            TVItemService tvItemService = new TVItemService(LanguageEnum.en, user);
            MapInfoService mapInfoService = new MapInfoService(LanguageEnum.en, user);

            TVItemModel tvItemModelRoot = tvItemService.GetRootTVItemModelDB();
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            List<Coord> coordList = new List<Coord>() { new Coord() { Lat = 50.0f, Lng = -90.0f, Ordinal = 0 } };

            MapInfoModel mapInfoModel = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.Root, tvItemModelRoot.TVItemID);
            if (!CheckModelOK<MapInfoModel>(mapInfoModel)) return false;

            TVItemModel tvItemModelCountry = tvItemService.PostCreateTVItem(tvItemModelRoot.TVItemID, "Canada", "Canada", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            coordList = new List<Coord>() { new Coord() { Lat = 52.0f, Lng = -90.0f, Ordinal = 0 } };

            mapInfoModel = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.Country, tvItemModelCountry.TVItemID);
            if (!CheckModelOK<MapInfoModel>(mapInfoModel)) return false;

            TVItemModel tvItemModelCountry2 = tvItemService.PostCreateTVItem(tvItemModelRoot.TVItemID, "United States", "Etats-Unis", TVTypeEnum.Country);
            if (!CheckModelOK<TVItemModel>(tvItemModelRoot)) return false;

            coordList = new List<Coord>() { new Coord() { Lat = 40.0f, Lng = -90.0f, Ordinal = 0 } };

            mapInfoModel = mapInfoService.CreateMapInfoObjectDB(coordList, MapInfoDrawTypeEnum.Point, TVTypeEnum.Country, tvItemModelCountry2.TVItemID);
            if (!CheckModelOK<MapInfoModel>(mapInfoModel)) return false;

            return true;
        }
    }
}
