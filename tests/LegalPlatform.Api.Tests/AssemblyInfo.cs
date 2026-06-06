using Xunit;

// These tests bootstrap the same in-process host (WebApplicationFactory<Program>) from multiple
// factories. Running them in parallel races the shared HostFactoryResolver state, which can cross
// configuration between factories. Serialize the assembly to keep host startup deterministic.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
