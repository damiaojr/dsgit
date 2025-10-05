using DS.Git.Cli;

// Create and execute command dispatcher
var dispatcher = new CommandDispatcher();
var exitCode = dispatcher.Dispatch(args);

return exitCode;