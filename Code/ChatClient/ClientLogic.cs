using System;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace ChatClient;

public partial class Form1 : Form
{
    private TextBox txtNickname, txtChatBox, txtInput;
    private Button btnConnect, btnSend;
    private ListBox lstOnline; 
    private TcpClient? client;
    private NetworkStream? stream;
    private bool isRunning = false;

    public Form1()
    {
        InitializeCustomComponents();
    }

    private void InitializeCustomComponents()
    {
        this.Text = "Chat Client - Member 4 & 5";
        this.Size = new Size(650, 600);
        this.StartPosition = FormStartPosition.CenterScreen;

        txtNickname = new TextBox { Location = new Point(20, 20), Width = 200, PlaceholderText = "Nhập Nickname..." };
        btnConnect = new Button { Location = new Point(230, 18), Text = "Kết nối", Width = 100, BackColor = Color.LightBlue };
        btnConnect.Click += BtnConnect_Click;

        txtChatBox = new TextBox { Location = new Point(20, 60), Size = new Size(420, 350), Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical };
        lstOnline = new ListBox { Location = new Point(450, 60), Size = new Size(160, 350) };

        txtInput = new TextBox { Location = new Point(20, 430), Size = new Size(420, 80), Multiline = true, PlaceholderText = "Nhập tin nhắn..." };
        btnSend = new Button { Location = new Point(450, 430), Size = new Size(160, 80), Text = "Gửi", Enabled = false, BackColor = Color.LightGreen };
        btnSend.Click += BtnSend_Click;

        this.Controls.AddRange(new Control[] { txtNickname, btnConnect, txtChatBox, lstOnline, txtInput, btnSend });
    }

    private void BtnConnect_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtNickname.Text)) return;

        try {
            client = new TcpClient("127.0.0.1", 8888);
            stream = client.GetStream();
            isRunning = true;

            // Gửi LOGIN 
            string loginMsg = $"LOGIN|{txtNickname.Text}|SERVER|Hello";
            byte[] data = Encoding.UTF8.GetBytes(loginMsg);
            stream.Write(data, 0, data.Length);

            btnConnect.Enabled = false;
            btnSend.Enabled = true;

            
            Task.Run(() => ReceiveData());
        }
        catch { MessageBox.Show("Không kết nối được server!"); }
    }

    private async Task ReceiveData()
    {
        byte[] buffer = new byte[1024];
        while (isRunning && client?.Connected == true)
        {
            try {
                int received = await stream!.ReadAsync(buffer, 0, buffer.Length);
                if (received == 0) break;

                string msg = Encoding.UTF8.GetString(buffer, 0, received);
                
               
                string[] parts = msg.Split('|');
                if (parts.Length < 4) continue;

                string command = parts[0];
                string sender = parts[1];
                string content = parts[3];

                this.Invoke(new Action(() => {
                    if (command == "LIST") {
                        lstOnline.Items.Clear();
                        foreach (var user in content.Split(',')) lstOnline.Items.Add(user);
                    } else {
                        txtChatBox.AppendText($"[{sender}]: {content}" + Environment.NewLine);
                    }
                }));
            }
            catch { break; }
        }
    }

    private void BtnSend_Click(object? sender, EventArgs e)
    {
        if (stream == null || string.IsNullOrWhiteSpace(txtInput.Text)) return;

        string target = lstOnline.SelectedItem?.ToString() ?? "ALL";
        // Gửi theo Format: COMMAND|SENDER|RECEIVER|CONTENT
        string msg = $"MSG|{txtNickname.Text}|{target}|{txtInput.Text}";
        byte[] data = Encoding.UTF8.GetBytes(msg);
        stream.Write(data, 0, data.Length);

        txtChatBox.AppendText($"[Tôi]: {txtInput.Text}" + Environment.NewLine);
        txtInput.Clear();
    }
}
