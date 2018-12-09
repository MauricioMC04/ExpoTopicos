using System;
using System.Messaging;
using System.Threading;
using System.Windows.Forms;

namespace ChatMessageQueue
{
	public partial class Chat : Form
	{
		public bool flag;
		public string name;
		public Thread chat;

		public Chat()
		{
			flag = false;
			TextBox.CheckForIllegalCrossThreadCalls = false;
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			listBox1.Items.Add("Digite su nombre");
		}

		private void groupBox1_Enter(object sender, EventArgs e)
		{

		}

		private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (flag)
			{
				string message = textBox1.Text;
				if (!message.Equals(""))
				{
					SendMessage(MessagePriority.Normal, textBox1.Text);
					textBox1.Text = "";
				}
			}
			else
			{
				name = textBox1.Text;
				textBox1.Text = "";
				listBox1.Items.Add("Hola " + name);
				listBox1.Items.Add("Chat Iniciado");
				flag = true;
				chat = new Thread(delegate () {
					while (true)
					{
						if (flag)
						{
							System.Messaging.Message message = ReceiveMessage();
							if (message != null)
							{
								listBox1.Items.Add(message.Label + ": " + message.Body);
							}
						}
						Thread.Sleep(20);
					}
				});
				chat.Start();
			}
		}

		public void CreateQueue()
		{
			using (MessageQueue queue = MessageQueue.Create(@".\private$\ChatQueue"))
			{
				queue.Label = "Chat Queue";
			}
		}

		public void SendMessage(MessagePriority priority, string messageBody)
		{
			if (!MessageQueue.Exists(@".\private$\ChatQueue"))
			{
				CreateQueue();
			}
			MessageQueue chatQueue = new MessageQueue(@".\private$\ChatQueue");
			System.Messaging.Message myMessage = new System.Messaging.Message();
			myMessage.Body = messageBody;
			myMessage.Priority = priority;
			myMessage.Label = name;
			chatQueue.Send(myMessage);
			listBox1.Items.Add("Yo: " + messageBody);
		}

		public System.Messaging.Message ReceiveMessage()
		{
			System.Messaging.Message myMessage = null;
			try
			{
				MessageQueue myQueue = new MessageQueue(@".\private$\ChatQueue");
				myQueue.MessageReadPropertyFilter.Priority = true;
				myQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
				myMessage = myQueue.Receive();
				if (myMessage.Label == name)
				{
					myQueue.Send(myMessage);
					myMessage = null;
				}
			}
			catch (MessageQueueException e)
			{
				Console.WriteLine(e.Message);
			}
			catch (InvalidOperationException e)
			{
				Console.WriteLine(e.Message);
			}
			return myMessage;
		}
	}
}
