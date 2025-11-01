using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Application.Abstractions
{

    public interface IActivationLinkService
    {
        Task<string> IssueLinkAsync(int requestId, CancellationToken ct);
        Task<int> ValidateAndConsumeAsync(string token, CancellationToken ct);
    }

}
