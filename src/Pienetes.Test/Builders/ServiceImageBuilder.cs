using Pienetes.App.Domain;

namespace Pienetes.Test.Builders
{
    public class ServiceImageBuilder
    {
        private string _name;
        private string _tag;

        public ServiceImageBuilder()
        {
            _name = "foo";
            _tag = "bar";
        }

        public ServiceImageBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ServiceImageBuilder WithTag(string tag)
        {
            _tag = tag;
            return this;
        }
        
        public ServiceImage Build()
        {
            return new ServiceImage(_name, _tag);
        }
    }
}