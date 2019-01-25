using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.Threading;

namespace CrossThreadingTests
{
    public partial class Form1 : Form
    {
        private readonly JoinableTaskFactory _joinableTaskFactory;

        public Form1()
        {
            InitializeComponent();

            _joinableTaskFactory  = new JoinableTaskFactory(new JoinableTaskContext());
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // await a completed task => will continue synchronously
            await SomeFastAsyncOperation().ConfigureAwait(false);

            // await a slow task => will continue in another thread
            var t = await SomeSlowAsyncOperation().ConfigureAwait(false);

            // write text to text box
            textBox1.Text = t;
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            // await a completed task => will continue synchronously
            await SomeFastAsyncOperation().ConfigureAwait(false);

            // await a slow task => will continue in another thread
            var t = SomeSlowAsyncOperation().ConfigureAwait(false).GetAwaiter().GetResult();

            // write text to text box
            textBox1.Text = t;
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            // force switch to threadpool thread
            await TaskScheduler.Default;

            // await a completed task => will continue synchronously
            await SomeFastAsyncOperation().ConfigureAwait(false);

            // await a slow task => will continue in another thread
            var t = await SomeSlowAsyncOperation().ConfigureAwait(false);

            // switch to main thread
            await _joinableTaskFactory.SwitchToMainThreadAsync();

            // write text to text box
            textBox1.Text = t;
        }

        private static Task<string> SomeFastAsyncOperation()
        {
            return Task.FromResult("test");
        }

        private async Task<string> SomeSlowAsyncOperation()
        {
            var httpClient = new HttpClient();
            var result = await httpClient.GetStringAsync("http://microsoft.com");
            return result;
        }
    }
}