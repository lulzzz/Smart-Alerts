namespace $safeprojectname$
{
    using Microsoft.Azure.Monitoring.SmartDetectors;

    /// <summary>
    /// The base class for representing a specific Alert.
    /// A specific implementation of an <see cref="Alert"/>. 
    /// The default implementation provides an alert without any additional data - only a title.
    /// </summary>
    public class MyAlert : Alert
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MyAlert"/> class.
        /// </summary>
        /// <param name="title">The Alert's title.</param>
        /// <param name="resourceIdentifier">The resource identifier that this Alert applies to.</param>
        public MyAlert(string title, ResourceIdentifier resourceIdentifier) : base(title, resourceIdentifier)
        {
        }
    }
}