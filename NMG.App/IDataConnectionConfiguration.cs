//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace NHibernateMappingGenerator
{
	public interface IDataConnectionConfiguration
	{
		string GetSelectedSource();
		void SaveSelectedSource(string provider);

		string GetSelectedProvider();
		void SaveSelectedProvider(string provider);
	}
}
