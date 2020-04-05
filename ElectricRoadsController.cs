
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System.IO;

namespace Klyte.ElectricRoads
{
    public class ElectricRoadsController : BaseController<ElectricRoadsMod, ElectricRoadsController>
    {
        public static readonly string FOLDER_NAME = "ElectricRoads";
        public static readonly string FOLDER_PATH = FileUtils.BASE_FOLDER_PATH + FOLDER_NAME;
        public static readonly string DEFAULT_CONFIG_FILE = $"{FOLDER_PATH}{Path.DirectorySeparatorChar}DefaultConfiguration.xml";
    }
}
