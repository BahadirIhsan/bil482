# IndustryApp web imajı (SDD 15.2): derlenmiş SPA + Caddy (HTTPS, reverse proxy).
# Derleme context'i: IndustryApp/ (Caddyfile deploy/ altında olduğu için).

FROM node:20-alpine AS build
WORKDIR /web
COPY src/frontend/industryapp-web/package.json src/frontend/industryapp-web/package-lock.json ./
RUN npm ci
COPY src/frontend/industryapp-web/ ./
RUN npm run build

FROM caddy:2-alpine
COPY deploy/Caddyfile /etc/caddy/Caddyfile
COPY --from=build /web/dist /srv
