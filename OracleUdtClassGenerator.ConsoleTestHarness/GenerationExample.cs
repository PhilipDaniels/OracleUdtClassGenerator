using System.Diagnostics;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

// This was copied and pasted from the Analyzers
// node so I can link it on my blog post.

namespace OracleUdts
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public partial class SkuRecord : IOracleCustomType, INullable
    {
        [OracleObjectMapping("ACTIVITY")]
        public string Activity { get; set; }

        [OracleObjectMapping("BARCODE")]
        public string Barcode { get; set; }

        [OracleObjectMapping("CATEGORY")]
        public string Category { get; set; }

        [OracleObjectMapping("COLORDESCRIPTION")]
        public string ColorDescription { get; set; }

        [OracleObjectMapping("DESCRIPTION")]
        public string Description { get; set; }

        [OracleObjectMapping("HEIGHT")]
        public decimal? Height { get; set; }

        [OracleObjectMapping("LENGTH")]
        public decimal? Length { get; set; }

        [OracleObjectMapping("MERCHANDISETYPE")]
        public string MerchandiseType { get; set; }

        [OracleObjectMapping("SIZEDESCRIPTION")]
        public string SizeDescription { get; set; }

        [OracleObjectMapping("SKU")]
        public string Sku { get; set; }

        [OracleObjectMapping("STYLEDESCRIPTION")]
        public string StyleDescription { get; set; }

        [OracleObjectMapping("SUBCATEGORY")]
        public string SubCategory { get; set; }

        [OracleObjectMapping("THUMBNAIL")]
        public string Thumbnail { get; set; }

        [OracleObjectMapping("TRAYITEM")]
        public int TrayItem { get; set; }

        [OracleObjectMapping("VOLUME")]
        public decimal? Volume { get; set; }

        [OracleObjectMapping("WEIGHT")]
        public decimal? Weight { get; set; }

        [OracleObjectMapping("WIDTH")]
        public decimal? Width { get; set; }

        private bool objectIsNull;
        public bool IsNull => objectIsNull;
        public static SkuRecord Null => new() { objectIsNull = true };
        public override string ToString() => $"{Sku}, {Barcode}/{Category}/{Description}";

        public void FromCustomObject(OracleConnection con, object udt)
        {
            if (Activity == null)
                OracleUdt.SetValue(con, udt, "ACTIVITY", DBNull.Value);
            else
                OracleUdt.SetValue(con, udt, "ACTIVITY", Activity);

            if (Barcode == null)
                OracleUdt.SetValue(con, udt, "BARCODE", DBNull.Value);
            else
                OracleUdt.SetValue(con, udt, "BARCODE", Barcode);

            if (Category == null)
                OracleUdt.SetValue(con, udt, "CATEGORY", DBNull.Value);
            else
                OracleUdt.SetValue(con, udt, "CATEGORY", Category);

            if (ColorDescription == null)
                OracleUdt.SetValue(con, udt, "COLORDESCRIPTION", DBNull.Value);
            else
                OracleUdt.SetValue(con, udt, "COLORDESCRIPTION", ColorDescription);

            if (Description == null)
                OracleUdt.SetValue(con, udt, "DESCRIPTION", DBNull.Value);
            else
                OracleUdt.SetValue(con, udt, "DESCRIPTION", Description);

            if (Height == null)
                OracleUdt.SetValue(con, udt, "HEIGHT", DBNull.Value);
            else
                OracleUdt.SetValue(con, udt, "HEIGHT", Height);

            if (Length == null)
                OracleUdt.SetValue(con, udt, "LENGTH", DBNull.Value);
            else
                OracleUdt.SetValue(con, udt, "LENGTH", Length);

            if (MerchandiseType == null)
                OracleUdt.SetValue(con, udt, "MERCHANDISETYPE", DBNull.Value);
            else
                OracleUdt.SetValue(con, udt, "MERCHANDISETYPE", MerchandiseType);

            if (SizeDescription == null)
                OracleUdt.SetValue(con, udt, "SIZEDESCRIPTION", DBNull.Value);
            else
                OracleUdt.SetValue(con, udt, "SIZEDESCRIPTION", SizeDescription);

            if (Sku == null)
                OracleUdt.SetValue(con, udt, "SKU", DBNull.Value);
            else
                OracleUdt.SetValue(con, udt, "SKU", Sku);

            if (StyleDescription == null)
                OracleUdt.SetValue(con, udt, "STYLEDESCRIPTION", DBNull.Value);
            else
                OracleUdt.SetValue(con, udt, "STYLEDESCRIPTION", StyleDescription);

            if (SubCategory == null)
                OracleUdt.SetValue(con, udt, "SUBCATEGORY", DBNull.Value);
            else
                OracleUdt.SetValue(con, udt, "SUBCATEGORY", SubCategory);

            if (Thumbnail == null)
                OracleUdt.SetValue(con, udt, "THUMBNAIL", DBNull.Value);
            else
                OracleUdt.SetValue(con, udt, "THUMBNAIL", Thumbnail);

            OracleUdt.SetValue(con, udt, "TRAYITEM", TrayItem);

            if (Volume == null)
                OracleUdt.SetValue(con, udt, "VOLUME", DBNull.Value);
            else
                OracleUdt.SetValue(con, udt, "VOLUME", Volume);

            if (Weight == null)
                OracleUdt.SetValue(con, udt, "WEIGHT", DBNull.Value);
            else
                OracleUdt.SetValue(con, udt, "WEIGHT", Weight);

            if (Width == null)
                OracleUdt.SetValue(con, udt, "WIDTH", DBNull.Value);
            else
                OracleUdt.SetValue(con, udt, "WIDTH", Width);
        }

        public void ToCustomObject(OracleConnection con, object udt)
        {
            if (OracleUdt.IsDBNull(con, udt, "ACTIVITY"))
                Activity = null;
            else
                Activity = (string)OracleUdt.GetValue(con, udt, "ACTIVITY");

            if (OracleUdt.IsDBNull(con, udt, "BARCODE"))
                Barcode = null;
            else
                Barcode = (string)OracleUdt.GetValue(con, udt, "BARCODE");

            if (OracleUdt.IsDBNull(con, udt, "CATEGORY"))
                Category = null;
            else
                Category = (string)OracleUdt.GetValue(con, udt, "CATEGORY");

            if (OracleUdt.IsDBNull(con, udt, "COLORDESCRIPTION"))
                ColorDescription = null;
            else
                ColorDescription = (string)OracleUdt.GetValue(con, udt, "COLORDESCRIPTION");

            if (OracleUdt.IsDBNull(con, udt, "DESCRIPTION"))
                Description = null;
            else
                Description = (string)OracleUdt.GetValue(con, udt, "DESCRIPTION");

            if (OracleUdt.IsDBNull(con, udt, "HEIGHT"))
                Height = null;
            else
                Height = (decimal?)OracleUdt.GetValue(con, udt, "HEIGHT");

            if (OracleUdt.IsDBNull(con, udt, "LENGTH"))
                Length = null;
            else
                Length = (decimal?)OracleUdt.GetValue(con, udt, "LENGTH");

            if (OracleUdt.IsDBNull(con, udt, "MERCHANDISETYPE"))
                MerchandiseType = null;
            else
                MerchandiseType = (string)OracleUdt.GetValue(con, udt, "MERCHANDISETYPE");

            if (OracleUdt.IsDBNull(con, udt, "SIZEDESCRIPTION"))
                SizeDescription = null;
            else
                SizeDescription = (string)OracleUdt.GetValue(con, udt, "SIZEDESCRIPTION");

            if (OracleUdt.IsDBNull(con, udt, "SKU"))
                Sku = null;
            else
                Sku = (string)OracleUdt.GetValue(con, udt, "SKU");

            if (OracleUdt.IsDBNull(con, udt, "STYLEDESCRIPTION"))
                StyleDescription = null;
            else
                StyleDescription = (string)OracleUdt.GetValue(con, udt, "STYLEDESCRIPTION");

            if (OracleUdt.IsDBNull(con, udt, "SUBCATEGORY"))
                SubCategory = null;
            else
                SubCategory = (string)OracleUdt.GetValue(con, udt, "SUBCATEGORY");

            if (OracleUdt.IsDBNull(con, udt, "THUMBNAIL"))
                Thumbnail = null;
            else
                Thumbnail = (string)OracleUdt.GetValue(con, udt, "THUMBNAIL");

            TrayItem = (int)OracleUdt.GetValue(con, udt, "TRAYITEM");

            if (OracleUdt.IsDBNull(con, udt, "VOLUME"))
                Volume = null;
            else
                Volume = (decimal?)OracleUdt.GetValue(con, udt, "VOLUME");

            if (OracleUdt.IsDBNull(con, udt, "WEIGHT"))
                Weight = null;
            else
                Weight = (decimal?)OracleUdt.GetValue(con, udt, "WEIGHT");

            if (OracleUdt.IsDBNull(con, udt, "WIDTH"))
                Width = null;
            else
                Width = (decimal?)OracleUdt.GetValue(con, udt, "WIDTH");
        }
    }

    /// <summary>
    /// An Oracle factory for the SkuRecord type.
    /// This allows us to bind/create individual objects.
    /// </summary>
    [OracleCustomTypeMapping("MYSCHEMA.objSku")]
    public partial class SkuRecordFactory : IOracleCustomTypeFactory
    {
        public IOracleCustomType CreateObject()
        {
            return new SkuRecord();
        }
    }

    public partial class SkuRecordArray : IOracleCustomType, INullable
    {
        [OracleArrayMapping]
        public SkuRecord[] Rows;

        private bool objectIsNull;
        public bool IsNull => objectIsNull;
        public static SkuRecordArray Null => new() { objectIsNull = true };

        public void FromCustomObject(OracleConnection con, object udt)
        {
            OracleUdt.SetValue(con, udt, 0, Rows);
        }

        public void ToCustomObject(OracleConnection con, object udt)
        {
            Rows = (SkuRecord[])OracleUdt.GetValue(con, udt, 0);
        }
    }

    /// <summary>
    /// An Oracle factory for the SkuRecordArray type.
    /// This allows us to bind/create arrays of objects.
    /// </summary>
    [OracleCustomTypeMapping("MYSCHEMA.tblSku")]
    public partial class SkuRecordArrayFactory : IOracleCustomTypeFactory, IOracleArrayTypeFactory
    {
        public IOracleCustomType CreateObject()
        {
            return new SkuRecordArray();
        }

        public Array CreateArray(int numElems)
        {
            return new SkuRecordArray[numElems];
        }

        public Array CreateStatusArray(int numElems)
        {
            return null;
        }
    }
}
