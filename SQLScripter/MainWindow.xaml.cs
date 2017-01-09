using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.Serialization;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;

namespace SQLScripter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataTable Databases { get; set; }
        private Server Srv { get; set; }
        public ObservableCollection<DbObject> DbTables = new ObservableCollection<DbObject>();
        private ObservableCollection<DbObject> DbProcedures = new ObservableCollection<DbObject>();

        public MainWindow()
        {
            InitializeComponent();
            Srv = new Server(@"(local)\SQLEXPRESS");
            Srv.SetDefaultInitFields(typeof(View), "IsSystemObject");

            var con = new SqlConnection(@"Data Source=MAIN\SQLEXPRESS; Integrated Security=True;");
            con.Open();
            Databases = con.GetSchema("Databases");
            DbComboBox.DataContext = Databases;
            TablesList.DataContext = DbTables;
            DbComboBox.DisplayMemberPath = Databases.Columns[0].ToString();
        }

        bool Export()
        {
            Scripter scr = new Scripter(Srv);
            ScriptingOptions dropOptions = new ScriptingOptions
            {
                DriAll = true,
                ClusteredIndexes = true,
                Default = true,
                Indexes = true,
                IncludeHeaders = true,
                AppendToFile = true,
                ToFileOnly = true,
                ScriptData = false,
                ScriptSchema = true,
                ScriptDrops = true,
                IncludeDatabaseContext = true,
                FileName = @"e:\Temp\test.sql"
            };

            ScriptingOptions createInsertOptions = new ScriptingOptions
            {
                // TODO: TargetServerVersion https://msdn.microsoft.com/en-us/library/microsoft.sqlserver.management.smo.sqlserverversion.aspx

                DriAll = true,
                ClusteredIndexes = true,
                Default = true,
                Indexes = true,
                IncludeHeaders = true,
                AppendToFile = true,
                ToFileOnly = true,
                ScriptData = true,
                ScriptSchema = true,
                ScriptDrops = false,
                ContinueScriptingOnError = false,
                IncludeDatabaseContext = true,
                FileName = @"e:\Temp\test.sql"
            };

            Database db = Srv.Databases[DbComboBox.Text];
            foreach (DbObject dbo in DbTables)
            {
                var tb = db.Tables.ItemById(dbo.Id);
                Debug.WriteLine(tb.Name);
                if (tb.IsSystemObject == true || dbo.Selected == false) continue;
                scr.Options = dropOptions;
                scr.EnumScript(new Urn[] { tb.Urn });
                scr.Options = createInsertOptions;
                scr.EnumScript(new Urn[] { tb.Urn });
            }

            return true;
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            Export();
        }

        private void DbComboBox_OnDropDownClosed(object sender, EventArgs e)
        {
            var dbName = DbComboBox.Text;
            if (String.IsNullOrEmpty(dbName)) return;
            foreach (Table tb in Srv.Databases[dbName].Tables)
            {
                DbTables.Add(new DbObject { Id = tb.ID, Name = tb.Name, Selected = true });
            }
        }

        private void SelectNone_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (DbObject dbo in DbTables)
            {
                dbo.Selected = false;
            }
        }

        private void SelectAll_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (DbObject dbo in DbTables)
            {
                dbo.Selected = true;
            }
        }

        private void SaveSelection_OnClick(object sender, RoutedEventArgs e)
        {
            XmlSerializer serializer = new XmlSerializer(DbTables.GetType());
            using (StreamWriter writer = new StreamWriter(@"e:\Temp\selections.xml"))
            {
                serializer.Serialize(writer, DbTables);
            }

        }

        private void LoadSelection_OnClick(object sender, RoutedEventArgs e)
        {
            XmlSerializer serializer = new XmlSerializer(DbTables.GetType());
            using (FileStream fileStream = new FileStream(@"e:\Temp\selections.xml", FileMode.Open))
            {
                DbTables = (ObservableCollection<DbObject>)serializer.Deserialize(fileStream);
            }
        }
    }
}
