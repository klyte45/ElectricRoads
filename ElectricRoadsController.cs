
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;

namespace Klyte.ElectricRoads
{
    public class ElectricRoadsController : BaseController<ElectricRoadsMod, ElectricRoadsController>
    {
        public static readonly string FOLDER_NAME = "ElectricRoads";
        public static readonly string FOLDER_PATH = FileUtils.BASE_FOLDER_PATH + FOLDER_NAME;
    }
}
