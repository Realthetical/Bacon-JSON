using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BacoNetworksJSONWriter
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            createJSONButton.Enabled = false;
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        string CompleteJSON = string.Empty;

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if(githubBox.Text == "put-your-github-link-here")
            {
                MessageBox.Show("you need to put your link in first");
                return;
            }

            DataSet testSet = new DataSet("testSet");
            testSet.Namespace = "NetFrameWork";
            DataTable table = new DataTable();
            table.TableName = "modules";
            DataColumn idColumn = new DataColumn("id", typeof(string));
            DataColumn nameColumn = new DataColumn("name", typeof(string));
            DataColumn typeColumn = new DataColumn("type", typeof(string));
            DataColumn artifactColumn = new DataColumn("artifact", typeof(Dictionary<object, object>));

            table.Columns.Add(idColumn);
            table.Columns.Add(nameColumn);
            table.Columns.Add(typeColumn);
            table.Columns.Add(artifactColumn);
            testSet.Tables.Add(table);

            string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string files in FileList)
            {
                using (ZipFile zip = ZipFile.Read(files))
                {
                    zip.ExtractSelectedEntries("mcmod.info", ExtractExistingFileAction.OverwriteSilently);
                    string firstRead = File.ReadAllText("mcmod.info");
                    string parseMe = firstRead.Substring(1, firstRead.Length - 3);
                    try
                    {
                        var jRead = JsonConvert.DeserializeObject<dynamic>(parseMe);
                        string name = jRead.name.ToString();
                        string modid = jRead.modid.ToString();
                        string modFile = System.IO.Path.GetFileName(files);
                        modList.Items.Add(modFile);
                        FileInfo info = new FileInfo(files);

                        int size = (int)info.Length;
                        string MD5 = CalculateMD5(files);

                        Dictionary<object, object> Dict = new Dictionary<object, object>
                {
                    { "size", size },
                    { "MD5", MD5 },
                    { "url", githubBox.Text + modFile}
                };

                        DataRow newRow = table.NewRow();
                        newRow["id"] = modid;
                        newRow["name"] = name;
                        newRow["type"] = "ForgeMod";
                        newRow["artifact"] = Dict;
                        table.Rows.Add(newRow);
                    }
                    catch
                    {

                    }
                    
                }
                
            }

            testSet.AcceptChanges();
            CompleteJSON = JsonConvert.SerializeObject(testSet, Formatting.Indented);
            createJSONButton.Enabled = true;
        }

        string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "");
                }
            }
        }

        private void createJSONButton_Click(object sender, EventArgs e)
        {
            if ((textBox2.Text != "") && (CompleteJSON != ""))
            {
                File.WriteAllText(textBox2.Text + ".json", CompleteJSON);
            }
            
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            createJSONButton.Enabled = true;
        }
    }
}
