namespace Tools
{
    public abstract class UnitConvereter
    {
        //Agar units(AU, game units)
        protected const int agarUnit = 1;
        //Unity Units
        protected const int cm = 100 * agarUnit;//Note: CameraHeight = 1 Unity unit
                                                 //Scale units(Such as Cylinder, Sphere, Plane)
        protected const float scale = cm * 0.5f;
        //CellSize Units(look at notebook for ref if needed)
        protected const float cellUnit = 2.23606798f * agarUnit;
        public static float CmToAU(float cmUnit)
        {
            return cmUnit * cm / agarUnit;
        }
        public static float CmToScale(float cmUnit)
        {
            return cmUnit * cm / scale;
        }
        public static float CmToCellUnit(float cmUnit)
        {
            return cmUnit * cm / cellUnit;
        }
        public static float AUToCm(float aUUnit)
        {
            return aUUnit * agarUnit / cm;
        }
        public static float AUToScale(float aUUnit)
        {
            return aUUnit * agarUnit / scale;
        }
        public static float AUToCellUnit(float aUUnit)
        {
            return aUUnit * agarUnit / cellUnit;
        }
        public static float ScaleToCm(float scaleUnit)
        {
            return scaleUnit * scale / cm;
        }
        public static float ScaleToAU(float scaleUnit)
        {
            return scaleUnit * scale / agarUnit;
        }
        public static float ScaleToCellUnit(float scaleUnit)
        {
            return scaleUnit * scale / cellUnit;
        }
        public static float CellUnitToCm(float cellUnit)
        {
            return cellUnit * UnitConvereter.cellUnit / cm;
        }
        public static float CellUnitToAU(float cellUnit)
        {
            return cellUnit * UnitConvereter.cellUnit / agarUnit;
        }
        public static float CellUnitToScale(float cellUnit)
        {
            return cellUnit * UnitConvereter.cellUnit / scale;
        }
    }
}