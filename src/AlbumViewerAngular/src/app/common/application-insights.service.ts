// https://github.com/jdubois/spring-on-azure/blob/master/src/main/webapp/app/core/insights/application-insights.service.ts

import { Injectable } from '@angular/core';
import { ApplicationInsights, IExceptionTelemetry } from '@microsoft/applicationinsights-web';

@Injectable({
    providedIn: 'root'
  })
export class ApplicationInsightsService {

    private appInsights = new ApplicationInsights({
        config: {
            instrumentationKey: '9d6565dc-c40e-4c0f-a09c-d21d8d533108',
            enableAutoRouteTracking: true,
            enableCorsCorrelation: true
        }
    });

    constructor() {
        this.appInsights.loadAppInsights();
    }

    setUserId(userId: string) {
        this.appInsights.setAuthenticatedUserContext(userId);
    }

    clearUserId() {
        this.appInsights.clearAuthenticatedUserContext();
    }

    logPageView(name?: string, uri?: string) {
        this.appInsights.trackPageView({ name, uri });
    }

    trackError(error: Error) {
        let telemetry = {
            exception: error
        } as IExceptionTelemetry;
        this.appInsights.trackException(telemetry);
    }
}
