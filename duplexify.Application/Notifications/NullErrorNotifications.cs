using duplexify.Application.Contracts.Notifications;

namespace duplexify.Application.Notifications
{
    internal class NullErrorNotifications : IErrorNotifications
    {
        public void Send(string message)
        {
            // this method is empty on purpose, because this is just a NOP
        }
    }
}
