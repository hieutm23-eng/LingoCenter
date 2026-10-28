using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace LingoCenter.Services
{
    // Valid OpenXML workbook. Inline strings keep submitted values from becoming formulas.
    public static class ExcelExport
    {
        public static byte[] Create(IEnumerable<string[]> rows)
        {
            XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            XNamespace rel = "http://schemas.openxmlformats.org/package/2006/relationships";
            XNamespace doc = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
            XNamespace ct = "http://schemas.openxmlformats.org/package/2006/content-types";
            using (var memory = new MemoryStream())
            {
                using (var zip = new ZipArchive(memory, ZipArchiveMode.Create, true))
                {
                    Add(zip, "[Content_Types].xml", new XElement(ct + "Types", new XElement(ct + "Default", new XAttribute("Extension", "rels"), new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")), new XElement(ct + "Default", new XAttribute("Extension", "xml"), new XAttribute("ContentType", "application/xml")), new XElement(ct + "Override", new XAttribute("PartName", "/xl/workbook.xml"), new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml")), new XElement(ct + "Override", new XAttribute("PartName", "/xl/worksheets/sheet1.xml"), new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"))));
                    Add(zip, "_rels/.rels", new XElement(rel + "Relationships", new XElement(rel + "Relationship", new XAttribute("Id", "rId1"), new XAttribute("Type", doc.NamespaceName + "/officeDocument"), new XAttribute("Target", "xl/workbook.xml"))));
                    Add(zip, "xl/workbook.xml", new XElement(ns + "workbook", new XAttribute(XNamespace.Xmlns + "r", doc), new XElement(ns + "sheets", new XElement(ns + "sheet", new XAttribute("name", "LingoCenter"), new XAttribute("sheetId", 1), new XAttribute(doc + "id", "rId1")))));
                    Add(zip, "xl/_rels/workbook.xml.rels", new XElement(rel + "Relationships", new XElement(rel + "Relationship", new XAttribute("Id", "rId1"), new XAttribute("Type", doc.NamespaceName + "/worksheet"), new XAttribute("Target", "worksheets/sheet1.xml"))));
                    Add(zip, "xl/worksheets/sheet1.xml", new XElement(ns + "worksheet", new XElement(ns + "cols", new XElement(ns + "col", new XAttribute("min", 1), new XAttribute("max", 20), new XAttribute("width", 25), new XAttribute("customWidth", 1))), new XElement(ns + "sheetData", rows.Select(r => new XElement(ns + "row", r.Select(c => new XElement(ns + "c", new XAttribute("t", "inlineStr"), new XElement(ns + "is", new XElement(ns + "t", c ?? "")))))))));
                }

                return memory.ToArray();
            }
        }

        private static void Add(ZipArchive zip, string path, XElement xml)
        {
            using (var writer = new StreamWriter(zip.CreateEntry(path).Open(), new UTF8Encoding(false)))
                xml.Save(writer);
        }
    }
}
