namespace OldWebApp.Services
{
    public class WelcomeMessageSvc
    {
        private const string _welcomeMessageTemplate = "Hey {0}";
        private const string _welcomeMessageNoName = "Hey user";

        public string GetWelcomeMessage(string name)
        {
            string retval = string.Empty;

            if ((name != null) && (name != string.Empty))
            {
                retval = string.Format(_welcomeMessageTemplate, name);
            }
            else
            {
                retval = _welcomeMessageNoName;
            }

            return retval;
        }
    }
}
