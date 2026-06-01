module.exports = ({ env }) => ({
  // Enable and configure the GraphQL plugin.
  // The endpoint is served at http://<host>:<port>/graphql.
  graphql: {
    enabled: true,
    config: {
      // Expose the Apollo Sandbox / GraphQL Playground in development only.
      playgroundAlways: env('NODE_ENV') === 'development',
      // Allow introspection in development so tooling (e.g. StrawberryShake's
      // `dotnet graphql update`) can refresh the schema.
      introspection: true,
      // Default depth/complexity guards.
      depthLimit: 10,
      amountLimit: 100,
      apolloServer: {
        tracing: false,
      },
    },
  },
});
