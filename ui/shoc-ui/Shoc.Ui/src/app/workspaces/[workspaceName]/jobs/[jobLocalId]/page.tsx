import getIntl from "@/i18n/get-intl";
import { Metadata } from "next";
import ErrorScreen from "@/components/error/error-screen";
import { getByName } from "../../cached-workspace-actions";
import WorkspacePageWrapper from "../../_components/workspace-page-wrapper";
import WorkspacePageHeader from "@/components/general/workspace-page-header";
import WorkspacePageBreadcrumbs from "@/components/general/workspace-page-breadcrumbs";
import { BreadcrumbLink } from "@/components/ui/breadcrumb";
import { getJobByLocalId } from "../cached-job-actions";
import JobStatusBadge from "../_components/job-status-badge";
import SingleJobClientPage from "./_components/single-job-client-page";

export const dynamic = 'force-dynamic';

export async function generateMetadata(props: { params: Promise<any> }): Promise<Metadata> {
  const params = await props.params;

  const {
    workspaceName,
    jobLocalId
  } = params;

  const intl = await getIntl();
  const defaultTitle = intl.formatMessage({ id: 'jobs' });
  const title = jobLocalId ? `${intl.formatMessage({ id: 'jobs.job' })} ${jobLocalId} - ${workspaceName}` : defaultTitle;

  return {
    title
  }
}

export default async function WorkspaceJobPage(props: any) {
  const params = await props.params;

  const {
    workspaceName,
    jobLocalId
  } = params;

  const intl = await getIntl();
  const { data: workspace, errors: workspaceErrors } = await getByName(workspaceName);

  if (workspaceErrors) {
    return <ErrorScreen errors={workspaceErrors} />
  }

  const { data: job, errors: jobErrors } = await getJobByLocalId(workspace.id, jobLocalId);

  if (jobErrors) {
    return <ErrorScreen errors={jobErrors} />
  }

  return <WorkspacePageWrapper header={
    <WorkspacePageHeader breadcrumb={
      <WorkspacePageBreadcrumbs crumbs={[
        <BreadcrumbLink key="jobs" href={`/workspaces/${job.workspaceName}/jobs`}>{intl.formatMessage({ id: 'jobs' })}</BreadcrumbLink>
      ]}
        title={`${jobLocalId}`} />
    }
    />
  }>
    <SingleJobClientPage />
  </WorkspacePageWrapper>
}
