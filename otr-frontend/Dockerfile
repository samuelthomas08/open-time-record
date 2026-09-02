FROM node:22-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM node:22-alpine AS final
WORKDIR /app
ENV NODE_ENV=production
COPY --from=build --chown=node:node /app/dist/otr-frontend/browser ./browser
COPY --from=build --chown=node:node /app/dist/otr-frontend/server ./server
USER node
EXPOSE 4000
CMD ["node", "server/server.mjs"]
