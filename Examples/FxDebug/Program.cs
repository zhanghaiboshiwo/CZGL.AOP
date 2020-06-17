﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CZGL.AOP;

namespace FxDebug
{
    public class LogAttribute : ActionAttribute
    {
        public override void Before(AspectContext context)
        {
            Console.WriteLine("执行前");
        }

        public override object After(AspectContext context)
        {
            Console.WriteLine("执行后");
            if (context.IsMethod)
                return context.MethodResult;
            else
                return context.PropertyValue;
        }
    }

    public interface ITest
    {
        void MyMethod(string a);
    }

    [Interceptor]
    public class Test : ITest
    {
        public Test()
        {
            Console.WriteLine("构造函数没问题");
        }
        [Log]
        public virtual void MyMethod(string a)
        {
            Console.WriteLine("运行中");
        }
    }

    public class TestAOPClass : Test
    {
        private readonly AspectContext _AspectContextBody;

        private LogAttribute _LogAttribute;

        public TestAOPClass():base()
        {
            _AspectContextBody = new AspectContextBody();
            ((AspectContextBody)_AspectContextBody).Type = GetType();
            ((AspectContextBody)_AspectContextBody).ConstructorParamters = new object[0];
            _LogAttribute = new LogAttribute();
        }

        public override void MyMethod(string a)
        {
            AspectContextBody newInstance = ((AspectContextBody)_AspectContextBody).NewInstance;
            newInstance.IsMethod = true;
            newInstance.MethodInfo = (MethodInfo)MethodBase.GetCurrentMethod();
            newInstance.MethodValues = new object[] { a};
            _LogAttribute.Before(newInstance);
            base.MyMethod((string)newInstance.MethodValues[0]);
            _LogAttribute.After(newInstance);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var s = typeof(Test).GetConstructors();
            var z= typeof(TestAOPClass).GetConstructors();
            var name = DynamicProxy.GetAssemblyName();
            var ab = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
            var am = ab.DefineDynamicModule("AOPDebugModule", "AOPDebug.dll");
            DynamicProxy.SetSave(ab, am);


            ITest test1 = AopInterceptor.CreateProxyOfInterface<ITest, Test>();
            Test test2 = AopInterceptor.CreateProxyOfClass<Test>();

            ab.Save("AopDebug.dll");

            test1.MyMethod("");
            test2.MyMethod("");

            Console.ReadKey();
        }
    }
}