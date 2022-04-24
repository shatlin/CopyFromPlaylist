using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Threading;

namespace CopyFromPlaylist
{

    public delegate string PlayListDelegate(string line);

    public partial class Form1 : Form
    {
        private DataTable SongDataTable;
        private string PlaylistPath;
        private string Playlistfolder;
        
        private string destinationFolder;

        public Form1()
        {
            InitializeComponent();
            PlaylistPath=string.Empty;
            if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)+"\\Playlists"))
            {
                textBox1.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)+"\\Playlists";

            }

            else if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\My Playlists"))
            {
                textBox1.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\ My Playlists";
            }

            else
            {
                textBox1.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            }

            destinationFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            textBox2.Text = destinationFolder;
          
            
        }
               
        private void btnSelectPlaylist_Click(object sender, EventArgs e)
        {

             
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            try
            {
                //  openFileDialog1.Filter = "m3u files (*.m3u)|*.m3u|wpl files (*.wpl)|*.wpl";
                openFileDialog1.Filter = "Plalist files (*.m3u,*.wpl)|*.m3u;*.wpl";
                openFileDialog1.FilterIndex = 2;
                
                openFileDialog1.InitialDirectory = textBox1.Text;

            }
            catch
            {
                if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\Playlists"))
                {
                    openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\Playlists";

                }

                else if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\My Playlists"))
                {
                    openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\ My Playlists";
                }

                else
                {
                    openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                }

            }

            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                PlayListCopier objPlaylistCopier = new PlayListCopier();
                PlaylistPath = System.IO.Path.GetFullPath(openFileDialog1.FileName);
                Playlistfolder = PlaylistPath.Substring(0, PlaylistPath.LastIndexOf("\\"));
                textBox1.Text = PlaylistPath;

                try
                {
                        SongDataTable = objPlaylistCopier.GetSongCollectionFromPlayList(textBox1.Text);
                        BindDataGrid();
                        
                        
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Playlist not in correct format.Please select a valid playlist");
                }
            }

        }

        //select the destination folder
        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            //folderBrowserDialog1.RootFolder = Environment.SpecialFolder.MyDocuments;
            folderBrowserDialog1.ShowNewFolderButton = true;
            folderBrowserDialog1.Description = "Please select a folder to copy or move the songs";
            DialogResult result = folderBrowserDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                destinationFolder=folderBrowserDialog1.SelectedPath;
                textBox2.Text = destinationFolder;
            }
        }

        // start processing the playlist (copy, move or delete)
        private void button2_Click(object sender, EventArgs e)
        {
            label1.Text = string.Empty;
            PlayListCopier objPlayListCopier=new PlayListCopier();

            if (PlaylistPath.Length == 0 || destinationFolder.Length == 0)
            {
                MessageBox.Show("Please select the playlist and the destination folder");
                return;
            }

            ProcessPlaylist();
            FormatGridView();

        }

        public void ProcessPlaylist()
        {
            string FileOrigPath;
            string FileSubPath;
            string destinationFoldernew;
            string destinationFile;
            string s1=string.Empty;
            progressBar1.Minimum = 1;
            progressBar1.Maximum = SongDataTable.Rows.Count;
            PlayListCopier objPlaylistCopier = new PlayListCopier();
            // Process the list of files found in the directory. 
            string[] fileEntries = Directory.GetFiles(Playlistfolder);

            foreach (string fileName in fileEntries)
            {

             
                SongDataTable = objPlaylistCopier.GetSongCollectionFromPlayList(fileName);
                BindDataGrid();
                progressBar1.Value = 1;

                foreach (DataRow dtrow in SongDataTable.Rows)
                {

                    try
                    {
                        FileOrigPath = Convert.ToString(dtrow["Song Location"]);
                        FileSubPath = FileOrigPath.Substring(FileOrigPath.IndexOf("\\"), FileOrigPath.LastIndexOf("\\") - 1);
                        destinationFolder = fileName.Substring(0, fileName.LastIndexOf(".m3u"));
                        destinationFoldernew = fileName.Substring(0,fileName.LastIndexOf(".m3u"));
                        
                        Directory.CreateDirectory(destinationFoldernew);

                        if (!checkBox1.Checked)
                        {


                            destinationFile = destinationFolder + FileOrigPath.Substring(FileOrigPath.LastIndexOf("\\"));
                        }
                        else
                        {
                            if (!Directory.Exists(destinationFoldernew))
                            {
                                Directory.CreateDirectory(destinationFoldernew);

                            }
                            destinationFile = destinationFolder + FileOrigPath.Substring(FileOrigPath.IndexOf("\\"));
                        }



                        if (File.Exists(destinationFile))
                        {
                            dtrow["Status"] = "File Already Exists";
                        }
                        else
                        {
                            if (checkBox2.Checked)
                            {
                                File.Move(Convert.ToString(dtrow["Song Location"]), destinationFile);
                                dtrow["Status"] = "Moved Successsfully";
                            }
                            else
                            {

                                File.Copy(Convert.ToString(dtrow["Song Location"]), destinationFile, true);
                                dtrow["Status"] = "Copied Successsfully";
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        dtrow["Status"] = "The file doesnot exist";
                    }

                  // progressBar1.Value = Convert.ToInt32(dtrow["Sequence"]);
                   // label1.Text = Convert.ToInt32(Convert.ToInt32(dtrow["Sequence"]) / SongDataTable.Rows.Count) * 100 + " % completed";
                    SongDataTable = null;
                }
            }
        }

        public void BindDataGrid()
        {

            dataGridView1.DataSource = SongDataTable;
            dataGridView1.Columns["Sequence"].Width = 65;
            dataGridView1.Columns["Status"].Width = 130;
            
        }

       private void FormatGridView()
      {
        foreach (DataGridViewRow row in dataGridView1.Rows)
        {
            
                string status=dataGridView1.Rows[row.Index].Cells["Status"].FormattedValue.ToString();

                if (!(status.IndexOf("Success")>0))
                {

                    dataGridView1.Rows[row.Index].Cells[0].Style.ForeColor = Color.Red;
                    dataGridView1.Rows[row.Index].Cells[1].Style.ForeColor = Color.Red;
                    dataGridView1.Rows[row.Index].Cells[2].Style.ForeColor = Color.Red;
                }

        }
                    
      
    }

       private void textBox1_Enter(object sender, EventArgs e)
       {
         //  btnSelectPlaylist.Focus();
       }

       private void textBox2_Enter(object sender, EventArgs e)
       {
     //      button1.Focus();
           
       }

       private void Form1_Load(object sender, EventArgs e)
       {
           PlayListCopier objPlaylistCopier=new PlayListCopier();
           SongDataTable = objPlaylistCopier.GetEmptyPlayList();
           BindDataGrid();
       }


    }

    public class PlayListCopier
    {
        public DataTable GetEmptyPlayList()
        {
            DataTable SongDataTable = new DataTable();

            SongDataTable.TableName = "PlayList";
            SongDataTable.Columns.Add("Sequence");
            SongDataTable.Columns.Add("Song Location");
            SongDataTable.Columns.Add("Status");

            
            // Read and display lines from the file until the end of the file is reached.
            for (int i = 0; i < 16;i++)
            {
                try
                {
                    
                        DataRow drow = SongDataTable.NewRow();
                        drow["Sequence"] = null;
                        drow["Song Location"] = "";
                        drow["Status"] = "";
                        SongDataTable.Rows.Add(drow);
                  
                }
                catch (Exception ex)
                {

                }

            }

            return SongDataTable;
        }

        public DataTable GetSongCollectionFromPlayList(string sPlaylistFileName)
        {

            DataTable SongDataTable = new DataTable();
            StreamReader myStream = new StreamReader(sPlaylistFileName);
            string line;
            string Lineread = string.Empty;

            SongDataTable.TableName = "PlayList";
            SongDataTable.Columns.Add("Sequence");
            SongDataTable.Columns.Add("Song Location");
            SongDataTable.Columns.Add("Status");

            PlayListDelegate delReadPlayListLine = new PlayListDelegate(ReadWPLLine); 


            if (sPlaylistFileName.Substring(sPlaylistFileName.LastIndexOf(".") + 1) == "m3u")
            {
                delReadPlayListLine = new PlayListDelegate(ReadM3ULine);
            }

            

                        int songnum = 0;
                        // Read and display lines from the file until the end of the file is reached.
                        while ((line = myStream.ReadLine()) != null)
                        {
                            try
                            {
                                Lineread = delReadPlayListLine(line);

                                if (Lineread != string.Empty)
                                {
                                    DataRow drow = SongDataTable.NewRow();
                                    drow["Sequence"] = ++songnum;
                                    drow["Song Location"] = Lineread;
                                    drow["Status"] = "Not Started";
                                    SongDataTable.Rows.Add(drow);
                                }
                            }
                            catch (Exception ex)
                            {

                            }

                        }

                        

           

            return SongDataTable;
        }

        public string ReadM3ULine(string line)
        {

            if ((line.Length > 0) && (line.Substring(0, 1) != "#"))
            {
                return line;
            }

            return string.Empty;



        }

        public string ReadWPLLine(string line)
        {
            if (line.IndexOf("<media src=") > 0)
            {
                try
                {
                    line = line.Substring(line.IndexOf(":\\") - 1, line.IndexOf(".mp3") - line.IndexOf(":\\") + 5);
                }
                
                catch
                {
                    line = line.Substring(line.IndexOf(":\\") - 1, line.IndexOf(".MP3") - line.IndexOf(":\\") + 5);
                }

                line = line.Replace("&apos;", "'");
                line = line.Replace("&amp;", "&"); 
                return line;

            }
            return string.Empty;

        }

        
    }
}