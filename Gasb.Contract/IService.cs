using System;
using System.ServiceModel;

namespace Gasb.Contract
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        Guid Register(string eMail);

        [OperationContract]
        int GetNumber(Guid clientId);

        [OperationContract]
        void SaveResult(Guid clientId, int n, bool isPrime);

    }
}
