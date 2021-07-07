using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WPFFourier_Plotter
{
    public class Circle
    {
        public double radius { get; set; }
        public double frequency { get; set; }
    }

    public class CircleList
    {
        [XmlElement(Type = typeof(List<Circle>), ElementName = "CircleList")]
        public List<Circle> circles { get; set; }
        public void Save(string FileName)
        {
            try
            {
                using (FileStream stream = new FileStream(FileName, FileMode.Create))
                {
                    XmlSerializer xml = new XmlSerializer(typeof(CircleList));
                    xml.Serialize(stream, this);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}");
            }
        }
        public static CircleList Load(string Filename)
        {
            CircleList circleList = new CircleList();
            try
            {
                using (FileStream stream = new FileStream(Filename, FileMode.Open))
                {
                    XmlSerializer xml = new XmlSerializer(typeof(CircleList));
                    circleList = (CircleList)xml.Deserialize(stream);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}");
            }
            return circleList;
        }
    }
}
