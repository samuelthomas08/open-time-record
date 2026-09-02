import { mergeApplicationConfig, ApplicationConfig } from '@angular/core';
import { provideServerRendering, withRoutes } from '@angular/ssr';
import { appConfig } from './app.config';
import { serverRoutes } from './app.routes.server';
import { provideApiConfiguration } from '@/api-client/api-configuration';

const serverConfig: ApplicationConfig = {
  providers: [
    provideServerRendering(withRoutes(serverRoutes)),
    // Overrides the browser-oriented (relative) API root URL for SSR: Node's fetch has no
    // implicit origin to resolve a relative URL against, so server-side rendering needs an
    // absolute address — the Docker Compose service name by default, direct container-to-
    // container call that also skips the reverse proxy.
    provideApiConfiguration(process.env['API_INTERNAL_URL'] ?? 'http://backend:8080'),
  ]
};

export const config = mergeApplicationConfig(appConfig, serverConfig);
