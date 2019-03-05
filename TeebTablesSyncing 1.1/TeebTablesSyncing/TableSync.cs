using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Timers;
using System.IO;
using System.Reflection;


namespace TeebTablesSyncing
{
    
    public partial class TableSync : Form
    {
        
        DataAccess ObjDataAccess = new DataAccess();
        
        public TableSync()
        {
            InitializeComponent();
           
        }
       // Disables the close button on the form
        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }
        private void TableSync_Load(object sender, EventArgs e)
        {
            //ObjDataAccess.ReadDatabase();
        }

        private void btnSync_Click(object sender, EventArgs e)
        {
            ObjDataAccess.ReadDatabase();
            //ObjDataAccess.GetDetailsFromServer();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
           // ObjDataAccess.ReadDatabase();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void TableSync_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();

        }

        private void toolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
