﻿using Microsoft.AspNetCore.Hosting;

namespace TASVideos.Extensions
{
    public static class HostingEnvironmentExtensions
    {
		public static bool IsLocalWithoutRecreate(this IHostingEnvironment env)
		{
			return env.IsEnvironment("Development-NoRecreate");
		}

		public static bool IsDemo(this IHostingEnvironment env)
		{
			return env.IsEnvironment("Demo");
		}

		public static bool IsAnyTestEnvironment(this IHostingEnvironment env)
		{
			return env.IsDevelopment()
				|| env.IsLocalWithoutRecreate()
				|| env.IsDemo();
		}
    }
}