﻿using Haondt.Core.Models;
using Haondt.Web.Core.Reasons;

namespace Haondt.Web.Assets
{
    public interface IAssetSource
    {
        public Task<Result<(byte[] Data, bool Cache), WebReason>> GetAssetAsync(string assetPath);
    }
}
