import getIntl from "@/i18n/get-intl";
import { Metadata } from "next";
import ErrorScreen from "@/components/error/error-screen";
import WorkspacePageHeader from "@/components/general/workspace-page-header";
import WorkspacePageBreadcrumbs from "@/components/general/workspace-page-breadcrumbs";
import { BreadcrumbLink } from "@/components/ui/breadcrumb";
import { getByName } from "@/app/workspaces/[workspaceName]/cached-workspace-actions";
import WorkspacePageWrapper from "@/app/workspaces/[workspaceName]/_components/workspace-page-wrapper";
import { getJobByLocalId } from "../../../../cached-job-actions";
import { getTaskBySequence } from "../../cached-job-task-actions";
import SingleTaskLogsClientPage from "./_components/single-task-logs-client-page";

export const dynamic = 'force-dynamic';

export async function generateMetadata(props: { params: Promise<any> }): Promise<Metadata> {
  const params = await props.params;

  const {
    workspaceName,
    sequence
  } = params;

  const intl = await getIntl();
  const title = `${intl.formatMessage({id: 'jobs.logs'})} - ${intl.formatMessage({ id: 'jobs.tasks.task' })} ${sequence} - ${workspaceName}`;

  return {
    title
  }
}

export default async function WorkspaceJobPage(props: any) {
  const params = await props.params;

  const {
    workspaceName,
    jobLocalId,
    sequence
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

  const { data: task, errors: taskErrors } = await getTaskBySequence(workspace.id, job.id, sequence);

  if (taskErrors) {
    return <ErrorScreen errors={taskErrors} />
  }

  return <WorkspacePageWrapper header={
    <WorkspacePageHeader breadcrumb={
      <WorkspacePageBreadcrumbs crumbs={[
        <BreadcrumbLink key="jobs" href={`/workspaces/${job.workspaceName}/jobs`}>
          {intl.formatMessage({ id: 'jobs' })}
        </BreadcrumbLink>,
        <BreadcrumbLink key="job" href={`/workspaces/${job.workspaceName}/jobs/${jobLocalId}`}>
          {intl.formatMessage({id: 'jobs.job'})} {jobLocalId}
        </BreadcrumbLink>,
        <BreadcrumbLink key="task" href={`/workspaces/${job.workspaceName}/jobs/${jobLocalId}/tasks/${sequence}`}>
          {intl.formatMessage({id: 'jobs.tasks.task'})} {sequence}
        </BreadcrumbLink>
      ]}
        title={intl.formatMessage({id: 'jobs.logs'})} />
    }
    />
  }>
      <SingleTaskLogsClientPage />

  </WorkspacePageWrapper>
}
