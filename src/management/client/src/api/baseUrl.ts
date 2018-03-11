// -----------------------------------------------------------------------
// <copyright file="baseUrl.ts" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

import baseUrlDev from './baseUrl.dev';

// Check which API url we should go by checking the environment name
const baseUrl = process.env.NODE_ENV === 'production' ? process.env.REACT_APP_FunctionBaseUrl : baseUrlDev;

export default baseUrl;