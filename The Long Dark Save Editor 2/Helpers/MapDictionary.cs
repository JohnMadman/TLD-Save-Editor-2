using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace The_Long_Dark_Save_Editor_2.Helpers
{
    public class MapInfo
    {
        public string inGameName;
        public Point origo;
        public int width;
        public int height;
        public float pixelsPerCoordinate;
        public Point ToRegion(Point point)
        {
            return new Point((point.X - origo.X) / pixelsPerCoordinate,
                    (point.Y - origo.Y) / -pixelsPerCoordinate);
        }
        public Point ToLayer(Point point)
        {
            return new Point(point.X * pixelsPerCoordinate + origo.X,
                    point.Y * -pixelsPerCoordinate + origo.Y);
        }
    }

    public static class MapDictionary
    {
        private static Dictionary<string, MapInfo> dict = new Dictionary<string, MapInfo>
        {
            { "AirfieldRegion", new MapInfo {origo = new Point(2046, 2041), width = 4096, height = 5042, pixelsPerCoordinate = 1.366153846153846f} },
            { "AshCanyonRegion", new MapInfo {origo = new Point(1746, 1721), width = 3500, height = 4086, pixelsPerCoordinate = 1.5f} },
            { "BlackrockRegion", new MapInfo {origo = new Point(1750, 1721), width = 3500, height = 4086, pixelsPerCoordinate = 1.5f} },
            { "CanneryRegion", new MapInfo {origo = new Point(2126, 2129), width = 3800, height = 3955, pixelsPerCoordinate = 1.5f} },
            { "CoastalRegion",new MapInfo {origo = new Point(2201, 2176), width = 4100, height = 3151, pixelsPerCoordinate = 1.5f} },
            { "CrashMountainRegion", new MapInfo {origo = new Point(89, 2988), width = 3165, height = 3575, pixelsPerCoordinate = 1.5f} },
            { "HighwayTransitionZone", new MapInfo {origo = new Point(108, 1732), width = 2100, height = 2310, pixelsPerCoordinate = 0.986f} },
            { "HubRegion", new MapInfo {origo = new Point(1746, 2509), width = 4096, height = 4942, pixelsPerCoordinate = 3.286f} },
            { "LakeRegion", new MapInfo {origo = new Point(397, 3065), width = 3246, height = 3500, pixelsPerCoordinate = 1.5f} },
            { "LongRailTransitionZone", new MapInfo {origo = new Point(1991, 4927), width = 4096, height = 8213, pixelsPerCoordinate = 3.375789473684211f} },
            { "MarshRegion", new MapInfo {origo = new Point(211, 3531), width = 3200, height = 3894, pixelsPerCoordinate = 1.5f} },
            { "MountainTownRegion", new MapInfo {origo = new Point(62, 3864), width = 3500, height = 4231, pixelsPerCoordinate = 1.5f} },
            { "RavineTransitionZone", new MapInfo {origo = new Point(1949, 830), width = 2350, height = 1464, pixelsPerCoordinate = 1.5f} },
            { "RiverValleyRegion", new MapInfo {origo = new Point(159, 2977), width = 3200, height = 3401, pixelsPerCoordinate = 1.5f} },
            { "RuralRegion", new MapInfo {origo = new Point(80, -124), width = 4950, height = 4926, pixelsPerCoordinate = 1.5f} },
            { "TracksRegion", new MapInfo {origo = new Point(-12, 459), width = 2600, height = 2325, pixelsPerCoordinate = 1.39625f} },
            { "WhalingStationRegion", new MapInfo {origo = new Point(174, 2730), width = 2610, height = 2751, pixelsPerCoordinate = 1.5f} },
            { "WindingRiverRegion", new MapInfo {origo = new Point(1436, 2218), width = 3850, height = 3325, pixelsPerCoordinate = 2.366666666666667f} },
            { "blank", new MapInfo {origo = new Point(1436, 2218), width = 3850, height = 3325, pixelsPerCoordinate = 1} },
        };

        public static List<string> MapNames
        {
            get { return dict.Keys.ToList(); }
        }

        public static MapInfo GetMapInfo(string mapName)
        {
            if(dict.TryGetValue(mapName, out var mapInfo))
                return mapInfo;
            return null;
        }

        public static bool MapExists(string region)
        {
            return dict.ContainsKey(region);
        }

        public static string GetInGameName(string region)
        {
            return Properties.Resources.ResourceManager.GetString(region) ?? region;
        }

    }
}
