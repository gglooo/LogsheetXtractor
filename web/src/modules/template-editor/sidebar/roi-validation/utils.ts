import type {
    RoiValidationConditionGroupType,
    RoiValidationConditionNodeType,
} from "@/modules/rois/validation/schema";

export const updateNodeAtPath = (
    node: RoiValidationConditionGroupType,
    path: number[],
    updater: (
        node: RoiValidationConditionNodeType,
    ) => RoiValidationConditionNodeType,
): RoiValidationConditionGroupType => {
    if (path.length === 0) {
        const updated = updater(node);
        return updated.type === "group" ? updated : node;
    }

    const [nextIndex, ...restPath] = path;
    const nextNode = node.children[nextIndex];

    if (!nextNode) {
        return node;
    }

    if (restPath.length > 0 && nextNode.type !== "group") {
        return node;
    }

    const updatedChildren = [...node.children];

    if (nextNode.type === "group") {
        updatedChildren[nextIndex] = updateNodeAtPath(
            nextNode,
            restPath,
            updater,
        );
    } else {
        updatedChildren[nextIndex] = updater(nextNode);
    }

    return {
        ...node,
        children: updatedChildren,
    };
};

export const removeNodeAtPath = (
    root: RoiValidationConditionGroupType,
    path: number[],
): RoiValidationConditionGroupType => {
    if (path.length === 0) {
        return root;
    }

    const parentPath = path.slice(0, -1);
    const removeIndex = path[path.length - 1];

    return updateNodeAtPath(root, parentPath, (node) => {
        if (node.type !== "group") {
            return node;
        }

        if (node.children.length <= 1) {
            return node;
        }

        return {
            ...node,
            children: node.children.filter(
                (_, childIndex) => childIndex !== removeIndex,
            ),
        };
    });
};
