using System;
using System.IO;
using System.Linq;
using System.Xml;
using SharpDox.Model.Repository;
using SharpDox.Sdk.Exporter;

namespace XMLExporter
{

    

    /// <summary>
    /// class to export the documentation into an xml file
    /// </summary>
    public class XMLExporter : IExporter
    {
        public event Action<string> OnStepMessage;
        public event Action<int> OnStepProgress;

        XmlDocument XML;

        protected void AddAttributeToNode(ref XmlNode Node, string AttributeName, string AttributeValue)
        {
            XmlAttribute Attrib = XML.CreateAttribute(AttributeName);
            Attrib.Value = AttributeValue == null ? "" : AttributeValue;
            Node.Attributes.Append(Attrib);
        }

        protected void AddInformationToNode(ref XmlNode Node, string Childname, string InformationData)
        {
            // add summary
            XmlNode NodeInfo = XML.CreateElement(Childname);
            XmlCDataSection CDATA = XML.CreateCDataSection(InformationData);
            NodeInfo.AppendChild(CDATA);
            Node.AppendChild(NodeInfo);
        }

        /// <summary>
        /// the code to do the export
        /// </summary>
        /// <param name="repository">reference to repository</param>
        /// <param name="outputPath">path to the output</param>
        public void Export(SDRepository repository, string outputPath)
        {
            // create an xml document from scratch
            XML = new XmlDocument();

            // the assembly node
            XmlNode NodeAssembly = XML.CreateElement("assembly");

            //////////////////////////////////////////////////////////////////////////
            // iterate all namespaces
            //////////////////////////////////////////////////////////////////////////
            System.Collections.Generic.List<SDNamespace> namespaces = repository.GetAllNamespaces();
            foreach (var nameSpace in namespaces)
            {
                // the namespace node
                XmlNode NodeNamespace = XML.CreateElement("namespace");
                AddAttributeToNode(ref NodeNamespace, "name", nameSpace.Fullname);


                //////////////////////////////////////////////////////////////////////////
                // iterate all classes
                //////////////////////////////////////////////////////////////////////////
                foreach (var type in nameSpace.Types)
                {
                    // copy the figures for the class
                    System.IO.Directory.CreateDirectory(Path.Combine(outputPath, "figures"));
                    File.Copy(type.ClassDiagramPath, Path.Combine(Path.Combine(outputPath, "figures"), new FileInfo(type.ClassDiagramPath).Name));

                    // class node
                    XmlNode NodeClass = XML.CreateElement("class");
                    // set name
                    AddAttributeToNode(ref NodeClass, "fullname", type.Fullname);
                    AddAttributeToNode(ref NodeClass, "name", type.Name);
                    AddAttributeToNode(ref NodeClass, "diagram", new FileInfo(type.ClassDiagramPath).Name);

                    if (type.Documentation != null)
                    {
                        AddInformationToNode(ref NodeClass, "summary", string.Join(" ", type.Documentation.Summary.Select(d => d.Text)));
                    }


                    //////////////////////////////////////////////////////////////////////////
                    // iterate all methods
                    //////////////////////////////////////////////////////////////////////////
                    foreach (var method in type.Methods)
                    {
                        // method node
                        XmlNode NodeMethod = XML.CreateElement("method");

                        AddAttributeToNode(ref NodeMethod, "name", method.Name);
                        AddAttributeToNode(ref NodeMethod, "returntype", method.ReturnType.Name);
                        AddInformationToNode(ref NodeMethod, "syntax", method.Syntax);
                       

                        if (method.Documentation != null)
                        {
                            if (method.Documentation.Summary != null && method.Documentation.Summary.Count > 0)
                                AddInformationToNode(ref NodeMethod, "summary", string.Join(" ", method.Documentation.Summary.Select(d => d.Text)));

                            if (method.Documentation.Remarks != null && method.Documentation.Remarks.Count >0)
                                AddInformationToNode(ref NodeMethod, "remarks", string.Join(" ", method.Documentation.Remarks.Select(d => d.Text)));

                            if (method.Documentation.Example != null && method.Documentation.Example.Count > 0)
                                AddInformationToNode(ref NodeMethod, "example", string.Join(" ", method.Documentation.Example.Select(d => d.Text)));

                            if (method.Documentation.Returns != null && method.Documentation.Returns.Count > 0)
                                AddInformationToNode(ref NodeMethod, "returns", string.Join(" ", method.Documentation.Returns.Select(d => d.Text)));

                            //////////////////////////////////////////////////////////////////////////
                            // iterate all parameters
                            //////////////////////////////////////////////////////////////////////////
                            foreach (var param in method.Parameters)
                            {

                                XmlNode NodeParameter = XML.CreateElement("parameter");
                                AddAttributeToNode(ref NodeParameter, "name", param.Name);
                                AddAttributeToNode(ref NodeParameter, "type", param.ParamType.Name);


                                if (method.Documentation != null)
                                {
                                    System.Collections.Generic.List<SharpDox.Model.Documentation.Token.SDToken> paramDoc = method.Documentation.Params.SingleOrDefault(p => p.Key == param.Name).Value;
                                    if (paramDoc != null)
                                    {
                                        foreach(SharpDox.Model.Documentation.Token.SDToken Tok in paramDoc){
                                            AddInformationToNode(ref NodeParameter, "summary", string.Join(" ", Tok.Text));
                                        }
                                    }
                                }
                                NodeMethod.AppendChild(NodeParameter);
                            }
                        }
                        NodeClass.AppendChild(NodeMethod);
                    }
                    NodeNamespace.AppendChild(NodeClass);
                }
                NodeAssembly.AppendChild(NodeNamespace);
            }


            XML.AppendChild(NodeAssembly);
            XML.Save(Path.Combine(outputPath, "project.xml"));
        }

        public string ExporterName { get { return "xml"; } }
    }
}
