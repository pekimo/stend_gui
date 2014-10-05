using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;

namespace Digital_Filte {

    public partial class Form1 : Form {
        public PingDevice PingDevice;
        public string dataport;
        
        private TextBox[] KIH;
        private TextBox[] BIH;
        private Label[] LabelKIH; 
        private Label[] LabelBIH;

        public Form1() {
            InitializeComponent();

            KIH = new[] { a0, a1, a2, a3, a4, a5, a6, a7, a8, a9 };
            BIH = new[] { a0_, b0_, a1_, b1_, a2_, b2_, a3_, b3_, a4_, b4_, a5_, b5_, a6_, b6_ };
            LabelKIH = new[] { labelA0, labelA1, labelA2, labelA3, labelA4, labelA5, labelA6, labelA7, labelA8, labelA9 };
            LabelBIH = new[] { labelA0_, labelB0_, labelA1_, labelB1_, labelA2_, labelB2_, labelA3_, labelB3_, labelA4_, labelB4_, labelA5_, labelB5_, labelA6_, labelB6_ };

            foreach (var textBox in KIH) {
                textBox.Text = "0";
                textBox.MaxLength = 10;
            }

            foreach (var textBox in BIH) {
                textBox.Text = "0";
                textBox.MaxLength = 10;
            }


            //задаем первоначальные значения коэффициентов фильтра, частоты генерации и разрядности.
            
            comboSamplingRate.SelectedIndex = 9;// comboSamplingRate.FindString("10");
            comboCapacity.SelectedIndex     = 15;// comboCapacity.FindString("16");
            comboOrderKIH.SelectedIndex     = 0;// comboOrderKIH.FindString("0");
            comboOrderBIH.SelectedIndex     = 0;// comboOrderBIH.FindString("0");
            comboBox1.SelectedIndex         = 0;
            a0.Text  = "1";
            a0_.Text = "1";
            b0_.Text = "1";
            
            PingDevice = new PingDevice();
            PingDevice.ComPort = port;

        }
        
        public void println(string msg) {
            //richTextBox1.AppendText(msg + "\n");
        }

        private void timer1_Tick(object sender, EventArgs e) {
            if (PingDevice.connection) {
                if (PingDevice.Ping()) return;
                else {
                    PingDevice.disconnection();
                    labelPingDevice.Text = "Стенд не подключен";
                    pictureBoxStatus.Image = imageListStatus.Images[1];
                    labelHelp.Text = " ";
                    return;
                }
            }
            else {
                int sign = PingDevice.IsConnection();

                if (sign == 0) {
                    labelPingDevice.Text = "Стенд не подключен";
                    pictureBoxStatus.Image = imageListStatus.Images[1];
                    return;
                }
                else if (sign == -1) {
                    labelPingDevice.Text = "Стенд не подключен";
                    pictureBoxStatus.Image = imageListStatus.Images[1];
                    return;
                }
                else if (sign == 1) {
                    PingDevice.connection = true;
                    labelPingDevice.Text = "Стенд подключен";
                    pictureBoxStatus.Image = imageListStatus.Images[0];
                    labelHelp.Text = " ";
                    try {
                        port.Write(Convert.ToString(comboSamplingRate.SelectedIndex + 1 + " "));
                        System.Threading.Thread.Sleep(100);
                        port.Write(Convert.ToString(comboCapacity.SelectedIndex + 1 + " "));
                    }
                    catch (Exception) { };
                    
                    return;
                }
            }
        }

        private void comboOrderKIH_SelectedIndexChanged(object sender, EventArgs e) {
            for (int i = comboOrderKIH.SelectedIndex; i < KIH.Length; ++i) {
                KIH[i].Visible = false;
                LabelKIH[i].Visible = false;
            }

            for (int i = 0; i <= comboOrderKIH.SelectedIndex; ++i) {
                KIH[i].Visible = true;
                LabelKIH[i].Visible = true;
            }      
                            
        }

        private void comboOrderBIH_SelectedIndexChanged(object sender, EventArgs e) {
            for (int i = (2 * comboOrderBIH.SelectedIndex + 2); i < BIH.Length; i += 2) {
                BIH[i].Visible = false;
                BIH[i + 1].Visible = false;
                LabelBIH[i].Visible = false;
                LabelBIH[i + 1].Visible = false;
            }
            for (int i = 0; i <= 2 * comboOrderBIH.SelectedIndex; i += 2) {
                BIH[i].Visible = true;
                BIH[i + 1].Visible = true;
                LabelBIH[i].Visible = true;
                LabelBIH[i + 1].Visible = true;
            }
        }

        private void comboSamplingRate_SelectedIndexChanged(object sender, EventArgs e) {
            try {
                port.Write(Convert.ToString(comboSamplingRate.SelectedIndex + 1 + " "));
            }
            catch (Exception) { };
        }//Частота дискретизации 

        private void comboCapacity_SelectedIndexChanged(object sender, EventArgs e) {
            try {
                port.Write(Convert.ToString(comboCapacity.SelectedIndex + 1 + " "));
            }
            catch (Exception) { };
        }//Разряднсть

        #region======================= Buttons ===========================

        private void button1_Click(object sender, EventArgs e) {
            if (tabControl1.SelectedIndex == 0) {
                foreach (var textBox in KIH)
                    textBox.Text = "0";
                a0.Text = "1";
            }

            if (tabControl1.SelectedIndex == 1) {
                foreach (var textBox in BIH)
                    textBox.Text = "0";
                a0_.Text = "1";
                b0_.Text = "1";
            }
        }//Очистка коэффициентов

        private void button2_Click(object sender, EventArgs e) {
            if (PingDevice.connection) {
                if (isCorrectInput()) {
                    PingDevice.packet.Coefdata = "";
                    if (tabControl1.SelectedIndex == 0) {
                        PingDevice.packet.Coefdata += string.Concat("K");
                        for (int i = 0; i < KIH.Length; ++i) {
                            if (comboBox1.SelectedIndex == 1)
                                PingDevice.packet.Coefdata += string.Concat(";", ScaledCoef(KIH[i].Text));
                            else
                                PingDevice.packet.Coefdata += string.Concat(";", KIH[i].Text);
                        }
                        PingDevice.packet.Coefdata += string.Concat(" ");
                        if (SendData(PingDevice.packet.Coefdata)) {
                            System.Threading.Thread.Sleep(100);
                            println(PingDevice.packet.KIHdata);
                        }
                    }
                    else if (tabControl1.SelectedIndex == 1) {
                        PingDevice.packet.Coefdata += string.Concat("B");
                        for (int i = 0; i < BIH.Length; ++i) {
                            if (comboBox1.SelectedIndex == 1)
                                PingDevice.packet.Coefdata += string.Concat(";", ScaledCoef(BIH[i].Text));
                            else
                                PingDevice.packet.Coefdata += string.Concat(";", BIH[i].Text);
                        }
                        PingDevice.packet.Coefdata += string.Concat(" ");
                        if (SendData(PingDevice.packet.Coefdata)) {
                            System.Threading.Thread.Sleep(100);
                            println(PingDevice.packet.BIHdata);
                        }
                    }
                }
            }
            else {
                labelHelp.Text = "Ошибка: подключите стенд для отправки данных.";
            }

        }//Кнопка применить коэффициенты

        private bool isCorrectInput() {
            if (tabControl1.SelectedIndex == 0) {
                foreach (var textBox in KIH)
                    if (textBox.BackColor == Color.Red) {
                        labelHelp.Text = "Ошибка: не корректный ввод коэффициентов.";
                        return false;
                    }
            }

            else if (tabControl1.SelectedIndex == 1){
                foreach (var textBox in BIH)
                    if (textBox.BackColor == Color.Red) {
                        labelHelp.Text = "Ошибка: не корректный ввод коэффициентов.";
                        return false;
                    }
            }
            return true;
        }//проверка корректного ввода коэффициентов
        
        private string ScaledCoef(string Acoef) {
            Double Adoub = Convert.ToDouble(Acoef) * 4096;
            return Convert.ToString((int)Adoub);
        }

        private bool SendData(string data) {
            try {
                //port.DiscardInBuffer();
                port.Write(data);
                return true;
            }
            catch (Exception) {
            println("ОШИБКА: устройствонеподключено");
            return false;
            }
        }//передача коэффицеентов

        #endregion

        private static void toRed(object sender) {
            try {
                int.Parse(((TextBox)sender).Text);
                ((TextBox)sender).BackColor = Color.White;
            }
            catch (FormatException) {
                ((TextBox)sender).BackColor = Color.Red;
            }
        }

        private void a__TextChanged(object sender, EventArgs e) {
            toRed(sender);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            try {
                port.Close();
                port = null;
                port.Dispose();
           }
           catch (Exception) { }
        }
        //Закрытие формы

    }

    #region=================== Classes ============================
    public class Packet {
        public string BIHdata;
        public string KIHdata;
        public string PINGdata;
        public string Coefdata;
        public Packet() {
            PINGdata = "PING";
        }
    }

    public class PingDevice {
        public SerialPort ComPort;
        public bool connection;
        public Packet packet;

        public PingDevice() {
            packet = new Packet();
            connection = false;
        }//Designer

        public bool Ping() {
            if (!connection) {
                return false;
            }
            string PING = "PING";
            //for (int i = 0; i < 3; i++)
            //{
            if (SendTo(PING + " ")) {
                //System.Threading.Thread.Sleep(100)
                if (PING.StartsWith(packet.PINGdata)) {
                    return true;
                }
            }
            System.Threading.Thread.Sleep(10);
            // }
            disconnection();
            return false;
        }

        public bool SendTo(string data) {
            try {
                byte[] buf = System.Text.Encoding.ASCII.GetBytes(data);
                ComPort.Write(buf, 0, data.Length);
                //System.Threading.Thread.Sleep(200)
                return true;
            }
            catch {
                return false;
            }
        }

        public int IsConnection() {
            try {
                string[] Ports = SerialPort.GetPortNames();
                int NPorts = Ports.Length;
                if (NPorts == 0) {
                    return 0;
                }
                foreach (string port in Ports) {
                    ComPort.PortName = port;
                    connection = true;
                    ComPort.Open();
                    if (Ping()) {
                        return 1;
                    }
                    disconnection();
                }
            }
            catch (Exception) {
                disconnection();
            }
            return -1;
        }

        public void disconnection() {
            try {
                ComPort.Close();
            }
            catch (Exception) { }
            connection = false;
        }
    }
    #endregion

}
