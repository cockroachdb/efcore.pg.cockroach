// For the following see https://github.com/aspnet/EntityFrameworkCore/issues/11848
// [assembly: CollectionBehavior(MaxParallelThreads = 1)]

[assembly: CollectionBehavior(DisableTestParallelization = true)]