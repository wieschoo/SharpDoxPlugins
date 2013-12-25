using System;
using System.IO;
using System.Linq;
using System.Xml;
using SharpDox.Model.Repository;
using SharpDox.Sdk.Exporter;

namespace XMLExporter
{

    

    /// <summary>
    /// class to export the documentation into an LaTeX source file
    /// </summary>
    public class LaTeXExporter : IExporter
    {
        public event Action<string> OnStepMessage;
        public event Action<int> OnStepProgress;

        string LaTeX = "";

        protected void AddLine(string Line)
        {
            LaTeX += Line + Enviroment.NewLine;
        }

        /// <summary>
        /// the code to do the export
        /// </summary>
        /// <param name="repository">reference to repository</param>
        /// <param name="outputPath">path to the output</param>
        public void Export(SDRepository repository, string outputPath)
        {

            string[] Preambel = new string[]{
                "\\documentclass[11pt,a4paper]{book}",
                "\\usepackage[utf8]{inputenc}",
                "\\usepackage[english]{babel}",
                "\\usepackage{amsmath}",
                "\\usepackage{amsfonts}",
                "\\usepackage{amssymb}",
                "\\usepackage{graphicx}",
                "\\usepackage{lmodern}",
                "\\usepackage[left=2cm,right=2cm,top=2cm,bottom=2cm]{geometry}",
                "\\author{SharpDox}",
                "",
                "\\title{Sharpdox}",
                "",
                "\\begin{document}",
                "\\maketitle",
                "\\tableofcontents",
                "\\newpage",
                "\\begin{document}"
            };

            Preambel.ForEach(line => AddLine(line));


            //////////////////////////////////////////////////////////////////////////
            // iterate all namespaces
            //////////////////////////////////////////////////////////////////////////
            System.Collections.Generic.List<SDNamespace> namespaces = repository.GetAllNamespaces();
            AddLine("\\section{Namespaces}");
            AddLine("\\begin{itemize}");
            repository.GetAllNamespaces().ForEach(item => AddLine("\\item "+item.Fullname ));
            AddLine("\\end{itemize}");

            // TODO


            AddLine("\\end{document}");
        }

        public string ExporterName { get { return "LaTeX"; } }
    }
}
