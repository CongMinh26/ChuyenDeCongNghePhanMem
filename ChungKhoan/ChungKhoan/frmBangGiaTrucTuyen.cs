using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChungKhoan
{
    public partial class frmBangGiaTrucTuyen : Form
    {
        private int changeCount = 0;
        private const String tableName = "BANGGIATRUCTUYEN";

        private SqlConnection connection = null;
        private SqlCommand command = null;
        private DataSet dataToWatch = null;
        public frmBangGiaTrucTuyen()
        {
            InitializeComponent();
        }

        private void bANGGIATRUCTUYENBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.bANGGIATRUCTUYENBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.cHUNGKHOANDataSet);

        }

        // cấp quyền
        private Boolean CanRequestNotifications()
        {
            try
            {
                SqlClientPermission perm = new SqlClientPermission(PermissionState.Unrestricted);
                perm.Demand();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void frmBangGiaTrucTuyen_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'cHUNGKHOANDataSet.BANGGIATRUCTUYEN' table. You can move, or remove it, as needed.
            this.bANGGIATRUCTUYENTableAdapter.Fill(this.cHUNGKHOANDataSet.BANGGIATRUCTUYEN);
            if (CanRequestNotifications() == true)
            {
                Program.KetNoi();
                BatDau();
            }
            else
            {
                MessageBox.Show("Bạn chưa kích hoạt dịch vụ Broker", "", MessageBoxButtons.OK);
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn thoát Form này không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Close();
            }
        }
        private String GetSQL()
        {
            return   "select MACP as [Mã CP],DM_GIA3 as [Giá 3],DM_KL3 as [KL 3]," +
                "DM_GIA2 as [Giá 2],DM_KL2 as [KL 2]," +
                "DM_GIA1 as [Giá 1],DM_KL1 as [KL 1]," +
                "KL_GIA as [Giá],KL_KL as [KL]," +
                "DB_GIA1 as [Giá 1],DB_KL1 as [KL 1]," +
                "DB_GIA2 as [Giá 2],DB_KL2 as [KL 2]," +
                "DB_GIA3 as [Giá 3],DB_KL3 as [KL 3] from dbo.BANGGIATRUCTUYEN"; 
        }

        private void dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            ISynchronizeInvoke i = (ISynchronizeInvoke)this;

            if (i.InvokeRequired)
            {
                OnChangeEventHandler tempDelegate = new OnChangeEventHandler(dependency_OnChange);

                object[] args = { sender, e };

                i.BeginInvoke(tempDelegate, args);
                return;
            }

            SqlDependency dependency = (SqlDependency)sender;

            dependency.OnChange -= dependency_OnChange;

            changeCount += 1;

            this.ListBox1.Items.Clear();
            this.ListBox1.Items.Add("Info:   " + e.Info.ToString());
            this.ListBox1.Items.Add("Source: " + e.Source.ToString());
            this.ListBox1.Items.Add("Type:   " + e.Type.ToString());

            GetData();
        }

        private void GetData()
        {
            dataToWatch.Clear();

            command.Notification = null;

            SqlDependency dependency = new SqlDependency(command);
            dependency.OnChange += dependency_OnChange;

            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                adapter.Fill(dataToWatch, tableName);
                this.DataGridView1.AutoGenerateColumns = false;
                this.DataGridView1.DataSource = dataToWatch;
                this.DataGridView1.DataMember = tableName;
            }
        }


        private void BatDau()
        {
            changeCount = 0;
            SqlDependency.Stop(Program.connectionString);
            try
            {
                SqlDependency.Start(Program.connectionString);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (connection == null)
            {
                connection = new SqlConnection(Program.connectionString);
                connection.Open();
            }
            if (command == null)
            {
                command = new SqlCommand(GetSQL(), connection);
            }

            if (dataToWatch == null)
            {
                dataToWatch = new DataSet();
            }

            GetData();
        }


    }
}
