// -----------------------------------------------------------------------
// <copyright file="baseUrl.ts" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

import baseUrlDev from './baseUrl.dev';

// Check which API url we should go by checking the environment name
const baseUrl = process.env.NODE_ENV === 'production' ? getProductionUrl() : baseUrlDev;

function getProductionUrl(): string {
    let currentHost = window.location.hostname;

    // The difference between the FE and BE regarding URL is the suffix.
    // E.g. - FE is https://lksdfji4jf-site.azurewebsites.net and BE is https://lksdfji4jf-fe.azurewebsites.net
    // We will do the convertion manually for now (TODO - make it configurable)
    let uniqueSiteNameRegex = new RegExp('(.*)-site.azurewebsites.net');
    let regexResults = uniqueSiteNameRegex.exec(currentHost);

    if (!regexResults || regexResults.length === 0) {
        throw 'Failed to retrieve production API url';
    }

    let uniqueSiteName = regexResults[1];

    return `https://${uniqueSiteName}-fa.azurewebsites.net`;
}

export default baseUrl;