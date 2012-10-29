using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ClearScada.Client;
using ClearScada.Client.Simple;

namespace ClearSCADA_Utilities
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonSubmit_Click(object sender, EventArgs e)
        {
            using (Connection conn = new Connection("myConn"))
            {
                conn.Connect(textBoxServer.Text);
                conn.LogOn(textBoxUsername.Text, textBoxPassword.Text);

                DBObject dbObj = null;

                try
                {
                    int id = Convert.ToInt32(textBoxID.Text);
                    dbObj = conn.GetObject(new ObjectId(id));
                    if (dbObj == null) throw new Exception("Invalid object ID.");
                    if (!dbObj.IsGroup) throw new Exception("Object is not a group.");
                }

                catch (FormatException ex)
                {
                    MessageBox.Show("Enter a valid object ID.");
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

                if (dbObj.ClassDefinition.Name == "CTemplateInstance") ProcessInstance(dbObj);
                else if (dbObj.ClassDefinition.Name == "CTemplate") ProcessTemplate(dbObj);
                
                conn.LogOff();
                conn.Disconnect();
            }
        }

        private void ProcessTemplate(DBObject template)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Fullname");
            dt.Columns.Add("Name");
            dt.Columns.Add("Template Address");

            //Todo: Find all instances & loop through them adding their addresses to grid

            dataGridViewResults.DataSource = dt;
        }

        private void ProcessInstance(DBObject instance)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Fullname");
            dt.Columns.Add("Name");
            dt.Columns.Add("Template Address");
            dt.Columns.Add("Instance Address");

            AddDescendants(ref dt, instance, "CSCADAPackMAnalogIn");
            AddDescendants(ref dt, instance, "CSCADAPackMAnalogOut");
            AddDescendants(ref dt, instance, "CSCADAPackMDigitalIn");
            AddDescendants(ref dt, instance, "CSCADAPackMDigitalOut");

            dataGridViewResults.DataSource = dt;
        }

        private void AddDescendants(ref DataTable dt, DBObject dbObj, string dbClass)
        {
            DBObjectCollection descendants = dbObj.GetDescendants(dbClass, "");
            
            foreach (DBObject descendant in descendants)
            {
                DBObject template = descendant.TemplateObject;
                string instanceAddress = descendant.GetProperty("Address").ToString();
                string templateAddress = template.GetProperty("Address").ToString();
                List<string> values = new List<string>();
                values.Add(template.FullName);
                values.Add(template.Name);
                values.Add(templateAddress);
                values.Add(instanceAddress);

                if (!checkBoxShowMismatchesOnly.Checked || (checkBoxShowMismatchesOnly.Checked && templateAddress != instanceAddress))
                {
                    dt.Rows.Add(values.ToArray());
                }
            }
        }
    }
}
